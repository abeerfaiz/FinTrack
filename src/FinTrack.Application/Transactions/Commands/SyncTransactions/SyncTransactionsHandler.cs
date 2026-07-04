using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Application.Common.Models;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using FinTrack.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinTrack.Application.Transactions.Commands.SyncTransactions;

public class SyncTransactionsHandler
    : IRequestHandler<SyncTransactionsCommand, Result<SyncTransactionsResult>>
{
    private readonly IBankConnectionRepository _bankConnectionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IOpenBankingClient _openBankingClient;
    private readonly ITokenEncryptionService _tokenEncryptionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SyncTransactionsHandler> _logger;
    private readonly ICategoryRulesEngine _categoryRulesEngine;

    public SyncTransactionsHandler(
        IBankConnectionRepository bankConnectionRepository,
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        IOpenBankingClient openBankingClient,
        ITokenEncryptionService tokenEncryptionService,
        IUnitOfWork unitOfWork,
        ICategoryRulesEngine categoryRulesEngine,
        ILogger<SyncTransactionsHandler> logger)
    {
        _bankConnectionRepository = bankConnectionRepository;
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _openBankingClient = openBankingClient;
        _tokenEncryptionService = tokenEncryptionService;
        _unitOfWork = unitOfWork;
        _categoryRulesEngine = categoryRulesEngine;
        _logger = logger;
    }

    public async Task<Result<SyncTransactionsResult>> Handle(
        SyncTransactionsCommand request,
        CancellationToken cancellationToken)
    {
        // ── Step 1: Load and validate bank connection ─────────────────────
        var connection = await _bankConnectionRepository
            .GetByIdAsync(request.BankConnectionId, cancellationToken);

        if (connection is null)
            throw new EntityNotFoundException(
                nameof(BankConnection), request.BankConnectionId);

        if (connection.Status != BankConnectionStatus.Active)
            return Result.Failure<SyncTransactionsResult>(
                $"Bank connection {request.BankConnectionId} is not active " +
                $"(status: {connection.Status}). User must re-authorise.");

        // ── Step 2: Decrypt access token ──────────────────────────────────
        string accessToken;
        try
        {
            accessToken = _tokenEncryptionService
                .Decrypt(connection.AccessTokenEncrypted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to decrypt access token for connection {ConnectionId}",
                request.BankConnectionId);
            return Result.Failure<SyncTransactionsResult>(
                "Failed to decrypt stored access token.");
        }

        // ── Step 3: Proactive token refresh ───────────────────────────────
        if (connection.IsTokenExpiringSoon())
        {
            _logger.LogInformation(
                "Token expiring soon for connection {ConnectionId}, refreshing proactively",
                request.BankConnectionId);

            var refreshToken = _tokenEncryptionService
                .Decrypt(connection.RefreshTokenEncrypted);

            var newTokens = await _openBankingClient
                .RefreshTokenAsync(refreshToken, cancellationToken);

            connection.UpdateTokens(
                _tokenEncryptionService.Encrypt(newTokens.AccessToken),
                _tokenEncryptionService.Encrypt(newTokens.RefreshToken),
                newTokens.ExpiresAt);

            accessToken = newTokens.AccessToken;
            _bankConnectionRepository.Update(connection);

            // Save refreshed tokens immediately — don't bundle with
            // account/transaction saves in case those fail
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // ── Step 4: Fetch accounts from TrueLayer ─────────────────────────
        var openBankingAccounts = await _openBankingClient
            .GetAccountsAsync(accessToken, cancellationToken);

        var accountsSynced = 0;
        var transactionsInserted = 0;
        var transactionsUpdated = 0;

        // ── Step 5: Build account map — upsert all accounts first ─────────
        // We build a map of externalAccountId → Account entity so we can
        // reference the correct account.Id when inserting transactions.
        // All accounts are tracked by EF Core and saved before any
        // transaction insert references their IDs — this is the correct
        // way to handle FK dependencies without raw SQL or tracker conflicts.
        var accountMap = new Dictionary<string, Account>();

        foreach (var obAccount in openBankingAccounts)
        {
            var account = await _accountRepository
                .GetByExternalAccountIdAsync(
                    obAccount.ExternalAccountId, cancellationToken);

            if (account is null)
            {
                if (!Enum.TryParse<AccountType>(
                    obAccount.AccountType, ignoreCase: true, out var accountType))
                    accountType = AccountType.Transaction;

                account = new Account(
                    bankConnectionId: connection.Id,
                    userId: connection.UserId,
                    externalAccountId: obAccount.ExternalAccountId,
                    providerId: obAccount.ProviderId,
                    accountType: accountType,
                    displayName: obAccount.DisplayName,
                    currency: obAccount.Currency,
                    tlUpdateTimestamp: obAccount.UpdateTimestamp,
                    sortCode: obAccount.SortCode,
                    accountNumber: obAccount.AccountNumber,
                    iban: obAccount.Iban,
                    swiftBic: obAccount.SwiftBic);

                await _accountRepository.AddAsync(account, cancellationToken);
            }

            // Fetch and update balance — failure here must not abort sync
            try
            {
                var balance = await _openBankingClient.GetBalanceAsync(
                    accessToken, obAccount.ExternalAccountId, cancellationToken);

                account.UpdateBalance(
                    balance.Current, balance.Available, balance.Overdraft);
                _accountRepository.Update(account);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to fetch balance for account {AccountId}, continuing",
                    obAccount.ExternalAccountId);
            }

            accountMap[obAccount.ExternalAccountId] = account;
            accountsSynced++;
        }

        // ── Step 6: Save all accounts ─────────────────────────────────────
        // All accounts must exist in the database before any transaction
        // can reference their IDs via the FK constraint.
        // This is the correct two-save pattern for FK-dependent data:
        // parent entities first, child entities second.
        // Both saves use the same EF Core DbContext and change tracker —
        // no raw SQL mixing, no tracker conflicts.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Saved {AccountCount} accounts for connection {ConnectionId}",
            accountsSynced, request.BankConnectionId);

        // ── Step 7: Process transactions per account ──────────────────────
        // Account entities are now in the database and also still tracked
        // by EF Core's change tracker from the same DbContext scope.
        // We use the accountMap built above — no second DB query needed
        // to retrieve accounts again, avoiding the stale entity problem.
        foreach (var (externalAccountId, account) in accountMap)
        {
            var fromDate = account.LastSyncedAt.HasValue
                ? account.LastSyncedAt.Value
                : DateTimeOffset.UtcNow.AddDays(-90);

            var openBankingTransactions = await _openBankingClient
                .GetTransactionsAsync(
                    accessToken,
                    externalAccountId,
                    from: fromDate,
                    cancellationToken: cancellationToken);

            foreach (var obTx in openBankingTransactions)
            {
                var existing = await _transactionRepository
                    .GetByExternalTxIdAsync(obTx.ExternalTxId, cancellationToken);

                if (existing is null)
                {
                    // New transaction — insert via EF Core
                    if (!Enum.TryParse<TransactionStatus>(
                        obTx.Status, ignoreCase: true, out var txStatus))
                        txStatus = TransactionStatus.Settled;

                    if (!Enum.TryParse<TransactionType>(
                        obTx.TransactionType, ignoreCase: true, out var txType))
                        txType = TransactionType.Debit;

                    var transaction = new Transaction(
                        accountId: account.Id,
                        userId: connection.UserId,
                        externalTxId: obTx.ExternalTxId,
                        status: txStatus,
                        transactionType: txType,
                        transactionCategory: obTx.TransactionCategory,
                        transactionClassification: obTx.TransactionClassification,
                        description: obTx.Description,
                        amount: obTx.Amount,
                        currency: obTx.Currency,
                        transactionDate: obTx.TransactionDate,
                        rawPayload: obTx.RawPayloadJson,
                        merchantName: obTx.MerchantName,
                        runningBalance: obTx.RunningBalance,
                        normalisedProviderTxId: obTx.NormalisedProviderTxId,
                        providerTransactionId: obTx.ProviderTransactionId);

                    // Run auto-categorisation rules engine on every new transaction.
                    // Returns null if no rule matches — transaction stays uncategorised.
                    // is_manually_categorised stays false — automation can improve
                    // the category later if the user adds a better rule.
                    var matchedCategoryId = await _categoryRulesEngine.FindMatchAsync(
                        connection.UserId,
                        obTx.MerchantName,
                        obTx.Description,
                        cancellationToken);

                    if (matchedCategoryId.HasValue)
                        transaction.AssignCategory(matchedCategoryId.Value, isManual: false);

                    await _transactionRepository.AddAsync(transaction, cancellationToken);
                    transactionsInserted++;
                }
                else if (existing.Status == TransactionStatus.Pending)
                {
                    // Pending transaction — may have settled or changed amount
                    if (Enum.TryParse<TransactionStatus>(
                        obTx.Status, ignoreCase: true, out var newStatus))
                    {
                        existing.UpdateFromProviderSync(
                            newStatus, obTx.Amount, obTx.RunningBalance);
                        _transactionRepository.Update(existing);
                        transactionsUpdated++;
                    }
                }
                // Settled transactions are immutable — skip entirely
            }

            // Mark account as synced — still tracked by EF Core
            // from the same DbContext, no stale entity issue
            account.RecordSync();
            _accountRepository.Update(account);
        }

        // ── Step 8: Save transactions and last_synced_at updates ──────────
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Sync complete for connection {ConnectionId}: " +
            "{AccountsSynced} accounts, {Inserted} inserted, {Updated} updated",
            request.BankConnectionId,
            accountsSynced,
            transactionsInserted,
            transactionsUpdated);

        return Result.Success(new SyncTransactionsResult(
            accountsSynced,
            transactionsInserted,
            transactionsUpdated));
    }
}
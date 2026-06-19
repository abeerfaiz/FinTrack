using FinTrack.Domain.Enums;
using FinTrack.Domain.Exceptions;

namespace FinTrack.Domain.Entities;

public class Transaction
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid UserId { get; private set; }

    // Identifiers from TrueLayer — used for idempotent sync
    public string ExternalTxId { get; private set; } = null!;
    public string? NormalisedProviderTxId { get; private set; }
    public string? ProviderTransactionId { get; private set; }

    // Core transaction facts — immutable once synced from the bank
    public TransactionStatus Status { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public string TransactionCategory { get; private set; } = null!; // PURCHASE / ATM / TRANSFER etc
    public IReadOnlyList<string> TransactionClassification { get; private set; } = new List<string>();
    public string? ProviderCategoryDisplay { get; private set; }
    public string Description { get; private set; } = null!;
    public string? MerchantName { get; private set; }
    public decimal Amount { get; private set; } // signed: negative = debit, positive = credit
    public string Currency { get; private set; } = null!;
    public DateTimeOffset TransactionDate { get; private set; }
    public decimal? RunningBalance { get; private set; }

    // User-controlled categorisation — mutable via explicit behaviour methods only
    public Guid? UserCategoryId { get; private set; }
    public bool IsManuallyCategorised { get; private set; }
    public bool IsArchived { get; private set; }

    public string RawPayload { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    private Transaction() { } // EF Core needs this — never call it yourself

    public Transaction(
        Guid accountId,
        Guid userId,
        string externalTxId,
        TransactionStatus status,
        TransactionType transactionType,
        string transactionCategory,
        IEnumerable<string> transactionClassification,
        string description,
        decimal amount,
        string currency,
        DateTimeOffset transactionDate,
        string rawPayload,
        string? merchantName = null,
        decimal? runningBalance = null,
        string? normalisedProviderTxId = null,
        string? providerTransactionId = null)
    {
        if (accountId == Guid.Empty)
            throw new DomainException("Transaction must belong to a valid account.");

        if (string.IsNullOrWhiteSpace(externalTxId))
            throw new DomainException("Transaction must have an external transaction id from the provider.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Transaction must specify a currency.");

        Id = Guid.NewGuid();
        AccountId = accountId;
        UserId = userId;
        ExternalTxId = externalTxId;
        NormalisedProviderTxId = normalisedProviderTxId;
        ProviderTransactionId = providerTransactionId;
        Status = status;
        TransactionType = transactionType;
        TransactionCategory = transactionCategory;

        var classificationList = transactionClassification?.ToList() ?? new List<string>();
        TransactionClassification = classificationList;
        ProviderCategoryDisplay = classificationList.Count > 0 ? classificationList[0] : null;

        Description = description;
        MerchantName = merchantName;
        Amount = amount;
        Currency = currency;
        TransactionDate = transactionDate;
        RunningBalance = runningBalance;
        RawPayload = rawPayload;

        UserCategoryId = null;
        IsManuallyCategorised = false;
        IsArchived = false;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void AssignCategory(Guid categoryId, bool isManual)
    {
        if (IsManuallyCategorised && !isManual)
            return; // never let automation overwrite a deliberate user choice

        UserCategoryId = categoryId;
        IsManuallyCategorised = isManual;
    }

    public void ClearCategory()
    {
        UserCategoryId = null;
        IsManuallyCategorised = false;
    }

    public void Archive() => IsArchived = true;
    public void Restore() => IsArchived = false;

    public void UpdateFromProviderSync(TransactionStatus status, decimal amount, decimal? runningBalance)
    {
        if (Status == TransactionStatus.Settled)
            throw new DomainException("Cannot modify a transaction that has already settled.");

        Status = status;
        Amount = amount;
        RunningBalance = runningBalance ?? RunningBalance;
    }
}
using FinTrack.Domain.Enums;
using FinTrack.Domain.Exceptions;

namespace FinTrack.Domain.Entities;

public class Account
{
    public Guid Id { get; private set; }
    public Guid BankConnectionId { get; private set; }
    public Guid UserId { get; private set; }

    // Identifiers from TrueLayer
    public string ExternalAccountId { get; private set; } = null!;
    public string ProviderId { get; private set; } = null!; // "monzo", "bank_of_scotland"

    public AccountType AccountType { get; private set; }
    public string DisplayName { get; private set; } = null!;
    public string Currency { get; private set; } = null!;

    // Account number details — all optional because not every
    // bank returns every field (swift_bic absent on single account
    // endpoint, as we saw in the actual TrueLayer responses)
    public string? SortCode { get; private set; }
    public string? AccountNumber { get; private set; }
    public string? Iban { get; private set; }
    public string? SwiftBic { get; private set; }

    // Three separate balance figures — all nullable because balance
    // is fetched in a separate API call from account details.
    // A freshly synced account may not have balance data yet.
    public decimal? BalanceCurrent { get; private set; }
    public decimal? BalanceAvailable { get; private set; }
    public decimal? BalanceOverdraft { get; private set; }
    public DateTimeOffset? BalanceUpdatedAt { get; private set; }

    public DateTimeOffset? LastSyncedAt { get; private set; }
    public DateTimeOffset TlUpdateTimestamp { get; private set; }

    // Navigation property — EF Core uses this to load related transactions
    // IReadOnlyList prevents external code from manipulating the collection directly
    private readonly List<Transaction> _transactions = new();
    public IReadOnlyList<Transaction> Transactions => _transactions;

    private Account() { }

    public Account(
        Guid bankConnectionId,
        Guid userId,
        string externalAccountId,
        string providerId,
        AccountType accountType,
        string displayName,
        string currency,
        DateTimeOffset tlUpdateTimestamp,
        string? sortCode = null,
        string? accountNumber = null,
        string? iban = null,
        string? swiftBic = null)
    {
        if (bankConnectionId == Guid.Empty)
            throw new DomainException("Account must belong to a valid bank connection.");

        if (string.IsNullOrWhiteSpace(externalAccountId))
            throw new DomainException("Account must have an external account id from the provider.");

        if (string.IsNullOrWhiteSpace(providerId))
            throw new DomainException("Account must have a provider id.");

        Id = Guid.NewGuid();
        BankConnectionId = bankConnectionId;
        UserId = userId;
        ExternalAccountId = externalAccountId;
        ProviderId = providerId;
        AccountType = accountType;
        DisplayName = displayName;
        Currency = currency;
        TlUpdateTimestamp = tlUpdateTimestamp;
        SortCode = sortCode;
        AccountNumber = accountNumber;
        Iban = iban;
        SwiftBic = swiftBic;
    }

    /// <summary>
    /// Updates balance from a separate TrueLayer balance API call.
    /// Called after the account sync job fetches balance data.
    /// All three figures are updated atomically — you never want
    /// current from one sync and available from a different one.
    /// </summary>
    public void UpdateBalance(
        decimal current,
        decimal available,
        decimal overdraft)
    {
        BalanceCurrent = current;
        BalanceAvailable = available;
        BalanceOverdraft = overdraft;
        BalanceUpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecordSync() => LastSyncedAt = DateTimeOffset.UtcNow;
}
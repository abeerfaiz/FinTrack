using FinTrack.Domain.Exceptions;

namespace FinTrack.Domain.Entities;

public class DirectDebit
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid UserId { get; private set; }
    public string ExternalDirectDebitId { get; private set; } = null!;
    public string Name { get; private set; } = null!; // "EE", "PAYPAL", "BT INTERNET"
    public string Status { get; private set; } = null!; // "Active" / "Inactive"
    public decimal PreviousPaymentAmount { get; private set; }
    public DateTimeOffset PreviousPaymentDate { get; private set; }
    public string Currency { get; private set; } = null!;
    public string RawPayload { get; private set; } = null!;
    public DateTimeOffset LastSyncedAt { get; private set; }

    private DirectDebit() { }

    public DirectDebit(
        Guid accountId,
        Guid userId,
        string externalDirectDebitId,
        string name,
        string status,
        decimal previousPaymentAmount,
        DateTimeOffset previousPaymentDate,
        string currency,
        string rawPayload)
    {
        if (accountId == Guid.Empty)
            throw new DomainException("Direct debit must belong to a valid account.");

        if (string.IsNullOrWhiteSpace(externalDirectDebitId))
            throw new DomainException("Direct debit must have an external id.");

        Id = Guid.NewGuid();
        AccountId = accountId;
        UserId = userId;
        ExternalDirectDebitId = externalDirectDebitId;
        Name = name;
        Status = status;
        PreviousPaymentAmount = previousPaymentAmount;
        PreviousPaymentDate = previousPaymentDate;
        Currency = currency;
        RawPayload = rawPayload;
        LastSyncedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateFromSync(
        string status,
        decimal previousPaymentAmount,
        DateTimeOffset previousPaymentDate,
        string rawPayload)
    {
        Status = status;
        PreviousPaymentAmount = previousPaymentAmount;
        PreviousPaymentDate = previousPaymentDate;
        RawPayload = rawPayload;
        LastSyncedAt = DateTimeOffset.UtcNow;
    }
}
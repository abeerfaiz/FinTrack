using FinTrack.Domain.Exceptions;

namespace FinTrack.Domain.Entities;

public class StandingOrder
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid UserId { get; private set; }
    public string Status { get; private set; } = null!;

    // ISO 20022 frequency string e.g. "IntrvlMnthDay:01:26"
    // meaning "monthly on the 26th". Stored raw — parsing this
    // into human-readable text happens in the Application layer.
    public string Frequency { get; private set; } = null!;
    public string? Reference { get; private set; }
    public string? Payee { get; private set; }
    public string Currency { get; private set; } = null!;

    public DateTimeOffset? NextPaymentDate { get; private set; }
    public decimal? NextPaymentAmount { get; private set; }
    public DateTimeOffset? FirstPaymentDate { get; private set; }
    public decimal? FirstPaymentAmount { get; private set; }
    public DateTimeOffset? FinalPaymentDate { get; private set; }
    public decimal? FinalPaymentAmount { get; private set; }

    public string RawPayload { get; private set; } = null!;
    public DateTimeOffset LastSyncedAt { get; private set; }

    private StandingOrder() { }

    public StandingOrder(
        Guid accountId,
        Guid userId,
        string status,
        string frequency,
        string currency,
        string rawPayload,
        string? reference = null,
        string? payee = null,
        DateTimeOffset? nextPaymentDate = null,
        decimal? nextPaymentAmount = null,
        DateTimeOffset? firstPaymentDate = null,
        decimal? firstPaymentAmount = null,
        DateTimeOffset? finalPaymentDate = null,
        decimal? finalPaymentAmount = null)
    {
        if (accountId == Guid.Empty)
            throw new DomainException("Standing order must belong to a valid account.");

        Id = Guid.NewGuid();
        AccountId = accountId;
        UserId = userId;
        Status = status;
        Frequency = frequency;
        Currency = currency;
        RawPayload = rawPayload;
        Reference = reference;
        Payee = payee;
        NextPaymentDate = nextPaymentDate;
        NextPaymentAmount = nextPaymentAmount;
        FirstPaymentDate = firstPaymentDate;
        FirstPaymentAmount = firstPaymentAmount;
        FinalPaymentDate = finalPaymentDate;
        FinalPaymentAmount = finalPaymentAmount;
        LastSyncedAt = DateTimeOffset.UtcNow;
    }
}
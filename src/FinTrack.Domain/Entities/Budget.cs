using FinTrack.Domain.Exceptions;

namespace FinTrack.Domain.Entities;

public class Budget
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CategoryId { get; private set; }
    public decimal Amount { get; private set; }

    // Always stored as the first day of the month.
    // e.g. June 2026 budget = new DateOnly(2026, 6, 1)
    // Makes range queries trivial: WHERE month_start = '2026-06-01'
    public DateOnly MonthStart { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public bool IsDeleted => DeletedAt.HasValue;

    private Budget() { }

    public Budget(
        Guid userId,
        Guid categoryId,
        decimal amount,
        DateOnly monthStart)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Budget must belong to a valid user.");

        if (categoryId == Guid.Empty)
            throw new DomainException("Budget must target a valid category.");

        if (amount <= 0)
            throw new DomainException("Budget amount must be greater than zero.");

        // Enforce first-of-month invariant regardless of what date was passed in.
        // If caller passes 2026-06-15, we silently normalise to 2026-06-01.
        // This prevents duplicate budgets for the same month stored with different day values.
        Id = Guid.NewGuid();
        UserId = userId;
        CategoryId = categoryId;
        Amount = amount;
        MonthStart = new DateOnly(monthStart.Year, monthStart.Month, 1);
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateAmount(decimal newAmount)
    {
        if (newAmount <= 0)
            throw new DomainException("Budget amount must be greater than zero.");

        Amount = newAmount;
    }

    public void Delete() => DeletedAt = DateTimeOffset.UtcNow;
    public void Restore() => DeletedAt = null;
}
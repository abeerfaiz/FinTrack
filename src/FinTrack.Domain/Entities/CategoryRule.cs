using FinTrack.Domain.Exceptions;

namespace FinTrack.Domain.Entities;

public class CategoryRule
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Keyword { get; private set; } = null!;

    // Lower number = higher priority.
    // When multiple rules match a transaction's merchant name,
    // the rule with the lowest priority number wins.
    public int Priority { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private CategoryRule() { }

    public CategoryRule(
        Guid userId,
        Guid categoryId,
        string keyword,
        int priority)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Category rule must belong to a valid user.");

        if (categoryId == Guid.Empty)
            throw new DomainException("Category rule must target a valid category.");

        if (string.IsNullOrWhiteSpace(keyword))
            throw new DomainException("Category rule must have a keyword.");

        if (priority < 0)
            throw new DomainException("Priority must be zero or a positive integer.");

        Id = Guid.NewGuid();
        UserId = userId;
        CategoryId = categoryId;
        Keyword = keyword.Trim().ToUpperInvariant();
        Priority = priority;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdatePriority(int newPriority)
    {
        if (newPriority < 0)
            throw new DomainException("Priority must be zero or a positive integer.");

        Priority = newPriority;
    }

    public void UpdateKeyword(string newKeyword)
    {
        if (string.IsNullOrWhiteSpace(newKeyword))
            throw new DomainException("Category rule must have a keyword.");

        Keyword = newKeyword.Trim().ToUpperInvariant();
    }
}
using FinTrack.Domain.Exceptions;

namespace FinTrack.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; }

    // Null for system categories (is_system = true).
    // Set for user-created categories.
    public Guid? UserId { get; private set; }

    public string Name { get; private set; } = null!;
    public string ColourHex { get; private set; } = null!;
    public string Icon { get; private set; } = null!;

    // System categories are seeded on deployment and shared across
    // all users. They cannot be deleted or renamed by users.
    public bool IsSystem { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }
    public bool IsDeleted => DeletedAt.HasValue;

    private Category() { }

    // Factory method for system categories — no userId, not deletable
    public static Category CreateSystemCategory(string name, string colourHex, string icon)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category must have a name.");

        return new Category
        {
            Id = Guid.NewGuid(),
            UserId = null,
            Name = name,
            ColourHex = colourHex,
            Icon = icon,
            IsSystem = true
        };
    }

    // Factory method for user-created categories
    public static Category CreateUserCategory(
        Guid userId,
        string name,
        string colourHex,
        string icon)
    {
        if (userId == Guid.Empty)
            throw new DomainException("User category must belong to a valid user.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category must have a name.");

        return new Category
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name,
            ColourHex = colourHex,
            Icon = icon,
            IsSystem = false
        };
    }

    public void Rename(string newName)
    {
        if (IsSystem)
            throw new DomainException("System categories cannot be renamed.");

        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("Category name cannot be empty.");

        Name = newName;
    }

    public void Delete()
    {
        if (IsSystem)
            throw new DomainException("System categories cannot be deleted.");

        DeletedAt = DateTimeOffset.UtcNow;
    }

    public void Restore() => DeletedAt = null;
}
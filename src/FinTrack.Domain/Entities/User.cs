using FinTrack.Domain.Exceptions;

namespace FinTrack.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private readonly List<BankConnection> _bankConnections = new();
    public IReadOnlyList<BankConnection> BankConnections => _bankConnections;
    public string? RefreshToken { get; private set; }
    public DateTimeOffset? RefreshTokenExpiresAt { get; private set; }
    
    private User() { }

    public User(string email, string passwordHash, string displayName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("User must have an email address.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("User must have a password hash.");

        Id = Guid.NewGuid();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        DisplayName = displayName;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateDisplayName(string newDisplayName)
    {
        if (string.IsNullOrWhiteSpace(newDisplayName))
            throw new DomainException("Display name cannot be empty.");

        DisplayName = newDisplayName;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdatePasswordHash(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("Password hash cannot be empty.");

        PasswordHash = newPasswordHash;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetRefreshToken(string refreshToken, DateTimeOffset expiresAt)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiresAt = expiresAt;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiresAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public bool IsRefreshTokenValid(string token)
    {
        var hash = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(token)));

        return RefreshToken == hash
            && RefreshTokenExpiresAt.HasValue
            && RefreshTokenExpiresAt.Value > DateTimeOffset.UtcNow;
    }
}
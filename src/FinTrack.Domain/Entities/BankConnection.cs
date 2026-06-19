using FinTrack.Domain.Enums;
using FinTrack.Domain.Exceptions;

namespace FinTrack.Domain.Entities;

public class BankConnection
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string ProviderId { get; private set; } = null!;

    // Tokens stored encrypted — the raw values here are the
    // encrypted ciphertext, never the plaintext token.
    // Decryption happens in Infrastructure via ITokenEncryptionService.
    public string AccessTokenEncrypted { get; private set; } = null!;
    public string RefreshTokenEncrypted { get; private set; } = null!;
    public DateTimeOffset TokenExpiresAt { get; private set; }

    // 90-day regulatory re-consent clock starts here.
    // When DateTimeOffset.UtcNow > ConsentCreatedAt.AddDays(90),
    // the user must re-authorise via TrueLayer OAuth2.
    public DateTimeOffset ConsentCreatedAt { get; private set; }

    public BankConnectionStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private readonly List<Account> _accounts = new();
    public IReadOnlyList<Account> Accounts => _accounts;

    private BankConnection() { }

    public BankConnection(
        Guid userId,
        string providerId,
        string accessTokenEncrypted,
        string refreshTokenEncrypted,
        DateTimeOffset tokenExpiresAt)
    {
        if (userId == Guid.Empty)
            throw new DomainException("Bank connection must belong to a valid user.");

        if (string.IsNullOrWhiteSpace(providerId))
            throw new DomainException("Bank connection must have a provider id.");

        if (string.IsNullOrWhiteSpace(accessTokenEncrypted))
            throw new DomainException("Bank connection must have an access token.");

        Id = Guid.NewGuid();
        UserId = userId;
        ProviderId = providerId;
        AccessTokenEncrypted = accessTokenEncrypted;
        RefreshTokenEncrypted = refreshTokenEncrypted;
        TokenExpiresAt = tokenExpiresAt;
        ConsentCreatedAt = DateTimeOffset.UtcNow;
        Status = BankConnectionStatus.Active;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Called by the token refresh background job when TrueLayer
    /// issues new tokens. Both tokens always refresh together —
    /// you never update one without the other.
    /// </summary>
    public void UpdateTokens(
        string newAccessTokenEncrypted,
        string newRefreshTokenEncrypted,
        DateTimeOffset newExpiry)
    {
        if (Status != BankConnectionStatus.Active)
            throw new DomainException("Cannot update tokens on an inactive bank connection.");

        AccessTokenEncrypted = newAccessTokenEncrypted;
        RefreshTokenEncrypted = newRefreshTokenEncrypted;
        TokenExpiresAt = newExpiry;
    }

    public void Revoke()
    {
        Status = BankConnectionStatus.Revoked;
        AccessTokenEncrypted = string.Empty;
        RefreshTokenEncrypted = string.Empty;
    }

    public void MarkExpired() => Status = BankConnectionStatus.Expired;

    public bool IsTokenExpiringSoon()
        => TokenExpiresAt <= DateTimeOffset.UtcNow.AddMinutes(5);

    public bool IsConsentExpired()
        => DateTimeOffset.UtcNow > ConsentCreatedAt.AddDays(90);
}
using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace FinTrack.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire recurring job that proactively refreshes TrueLayer tokens
/// expiring within the next 5 minutes. Runs every 4 minutes to ensure
/// no token expires between checks.
///
/// Proactive refresh is the correct pattern — waiting for a 401 during
/// a sync job would leave data partially synced. Refreshing ahead of
/// expiry prevents this entirely.
/// </summary>
public class TokenRefreshJob
{
    private readonly IBankConnectionRepository _bankConnectionRepository;
    private readonly IOpenBankingClient _openBankingClient;
    private readonly ITokenEncryptionService _tokenEncryptionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TokenRefreshJob> _logger;

    public TokenRefreshJob(
        IBankConnectionRepository bankConnectionRepository,
        IOpenBankingClient openBankingClient,
        ITokenEncryptionService tokenEncryptionService,
        IUnitOfWork unitOfWork,
        ILogger<TokenRefreshJob> logger)
    {
        _bankConnectionRepository = bankConnectionRepository;
        _openBankingClient = openBankingClient;
        _tokenEncryptionService = tokenEncryptionService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var expiringSoon = await _bankConnectionRepository
            .GetExpiringSoonAsync(cancellationToken);

        if (!expiringSoon.Any())
        {
            _logger.LogDebug("Token refresh job: no tokens expiring soon");
            return;
        }

        _logger.LogInformation(
            "Token refresh job: refreshing {Count} expiring tokens",
            expiringSoon.Count);

        foreach (var connection in expiringSoon)
        {
            try
            {
                var refreshToken = _tokenEncryptionService
                    .Decrypt(connection.RefreshTokenEncrypted);

                var newTokens = await _openBankingClient
                    .RefreshTokenAsync(refreshToken, cancellationToken);

                connection.UpdateTokens(
                    _tokenEncryptionService.Encrypt(newTokens.AccessToken),
                    _tokenEncryptionService.Encrypt(newTokens.RefreshToken),
                    newTokens.ExpiresAt);

                _bankConnectionRepository.Update(connection);

                _logger.LogInformation(
                    "Refreshed token for connection {ConnectionId}, " +
                    "new expiry: {ExpiresAt}",
                    connection.Id,
                    newTokens.ExpiresAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to refresh token for connection {ConnectionId}",
                    connection.Id);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Token refresh job complete");
    }
}
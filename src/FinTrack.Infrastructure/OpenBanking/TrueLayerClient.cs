using FinTrack.Application.Common.Interfaces;

namespace FinTrack.Infrastructure.OpenBanking;

/// <summary>
/// Implements IOpenBankingClient using TrueLayer's Data API.
/// Auth operations delegate to TrueLayerAuthService (token lifecycle).
/// Data operations (accounts, balances, transactions) are added on Day 3.
/// </summary>
public class TrueLayerClient : IOpenBankingClient
{
    private readonly TrueLayerAuthService _authService;

    public TrueLayerClient(TrueLayerAuthService authService)
    {
        _authService = authService;
    }

    public Task<string> GetAuthorisationUrlAsync(
        string state,
        CancellationToken cancellationToken = default)
    {
        var url = _authService.BuildAuthorisationUrl(state);
        return Task.FromResult(url);
    }

    public async Task<OpenBankingTokenResult> ExchangeAuthCodeAsync(
        string authorisationCode,
        CancellationToken cancellationToken = default)
    {
        var (accessToken, refreshToken, expiresAt) =
            await _authService.ExchangeAuthCodeAsync(authorisationCode, cancellationToken);

        return new OpenBankingTokenResult(accessToken, refreshToken, expiresAt);
    }

    public async Task<OpenBankingTokenResult> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var (newAccessToken, newRefreshToken, expiresAt) =
            await _authService.RefreshTokenAsync(refreshToken, cancellationToken);

        return new OpenBankingTokenResult(newAccessToken, newRefreshToken, expiresAt);
    }

    // Day 3 implementations — stubbed to throw until built
    public Task<IReadOnlyList<OpenBankingAccount>> GetAccountsAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException("GetAccountsAsync implemented on Day 3.");

    public Task<OpenBankingBalance> GetBalanceAsync(
        string accessToken,
        string accountId,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException("GetBalanceAsync implemented on Day 3.");

    public Task<IReadOnlyList<OpenBankingTransaction>> GetTransactionsAsync(
        string accessToken,
        string accountId,
        DateTimeOffset? from = null,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException("GetTransactionsAsync implemented on Day 3.");
}
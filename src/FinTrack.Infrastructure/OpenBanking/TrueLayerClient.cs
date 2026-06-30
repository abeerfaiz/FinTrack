using FinTrack.Application.Common.Interfaces;
using FinTrack.Infrastructure.OpenBanking.Mappers;
using FinTrack.Infrastructure.OpenBanking.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FinTrack.Infrastructure.OpenBanking;

public class TrueLayerClient : IOpenBankingClient
{
    private readonly HttpClient _httpClient;
    private readonly TrueLayerAuthService _authService;
    private readonly ILogger<TrueLayerClient> _logger;

    public TrueLayerClient(
        IHttpClientFactory httpClientFactory,
        TrueLayerAuthService authService,
        ILogger<TrueLayerClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient("TrueLayerDataApi");
        _authService = authService;
        _logger = logger;
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

    public async Task<IReadOnlyList<OpenBankingAccount>> GetAccountsAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/data/v1/accounts");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException(
                "TrueLayer access token has expired. Token refresh required.");

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<TrueLayerAccountListResponse>(cancellationToken: cancellationToken);

        return result?.Results
            .Select(a => a.ToOpenBankingAccount())
            .ToList()
            ?? new List<OpenBankingAccount>();
    }

    public async Task<OpenBankingBalance> GetBalanceAsync(
        string accessToken,
        string accountId,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/data/v1/accounts/{accountId}/balance");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException(
                "TrueLayer access token has expired. Token refresh required.");

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<TrueLayerBalanceListResponse>(cancellationToken: cancellationToken);

        var balance = result?.Results.FirstOrDefault()
            ?? throw new InvalidOperationException(
                $"TrueLayer returned no balance data for account {accountId}.");

        return new OpenBankingBalance(
            balance.Current,
            balance.Available,
            balance.Overdraft,
            balance.Currency);
    }

    public async Task<IReadOnlyList<OpenBankingTransaction>> GetTransactionsAsync(
        string accessToken,
        string accountId,
        DateTimeOffset? from = null,
        CancellationToken cancellationToken = default)
    {
        var transactions = new List<OpenBankingTransaction>();

        var settledUrl = $"/data/v1/accounts/{accountId}/transactions";
        if (from.HasValue)
            settledUrl += $"?from={from.Value:yyyy-MM-dd}";

        var settled = await FetchTransactionsAsync(
            settledUrl, accessToken, status: "Settled", cancellationToken);
        transactions.AddRange(settled);

        var pendingUrl = $"/data/v1/accounts/{accountId}/transactions/pending";

        var pending = await FetchTransactionsAsync(
            pendingUrl, accessToken, status: "Pending", cancellationToken);
        transactions.AddRange(pending);

        return transactions;
    }

    private async Task<IReadOnlyList<OpenBankingTransaction>> FetchTransactionsAsync(
        string url,
        string accessToken,
        string status,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException(
                "TrueLayer access token has expired. Token refresh required.");

        if (!response.IsSuccessStatusCode)
        {
            // Pending endpoint can legitimately return no data
            // for some providers — log and continue rather than
            // failing the whole sync over a missing pending list.
            _logger.LogWarning(
                "TrueLayer {Url} returned {StatusCode}",
                url, response.StatusCode);
            return new List<OpenBankingTransaction>();
        }

        var result = await response.Content
            .ReadFromJsonAsync<TrueLayerTransactionListResponse>(cancellationToken: cancellationToken);

        return result?.Results
            .Select(t => t.ToOpenBankingTransaction(status))
            .ToList()
            ?? new List<OpenBankingTransaction>();
    }
}
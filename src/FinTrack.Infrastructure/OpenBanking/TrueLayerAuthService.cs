using FinTrack.Application.Common.Interfaces;
using FinTrack.Infrastructure.OpenBanking.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

namespace FinTrack.Infrastructure.OpenBanking;

/// <summary>
/// Handles TrueLayer-specific OAuth2 operations — building the
/// authorisation URL and exchanging auth codes / refresh tokens
/// for new token pairs. Split from TrueLayerClient deliberately:
/// auth concerns (token lifecycle) are separate from data concerns
/// (fetching accounts/transactions). Single responsibility.
/// </summary>
public class TrueLayerAuthService
{
    private readonly HttpClient _httpClient;
    private readonly TrueLayerOptions _options;

    public TrueLayerAuthService(
        HttpClient httpClient,
        IOptions<TrueLayerOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public string BuildAuthorisationUrl(string state)
    {
        var encodedRedirectUri = Uri.EscapeDataString(_options.RedirectUri);
        var encodedState = Uri.EscapeDataString(state);

        // Space-separated scope as TrueLayer docs specify
        var scope = Uri.EscapeDataString("info accounts balance transactions offline_access");

        // Space-separated providers — uk-ob-all for real banks,
        // uk-cs-mock enables the Mock Bank for sandbox testing
        var providers = Uri.EscapeDataString("uk-ob-all uk-cs-mock");

        var url = $"{_options.AuthBaseUrl}/?" +
                  $"response_type=code" +
                  $"&client_id={_options.ClientId}" +
                  $"&scope={scope}" +
                  $"&redirect_uri={encodedRedirectUri}" +
                  $"&providers={providers}" +
                  $"&state={encodedState}";

        return url;
    }

    public async Task<(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt)>
        ExchangeAuthCodeAsync(string code, CancellationToken cancellationToken)
    {
        var tokenRequest = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("client_id", _options.ClientId),
            new KeyValuePair<string, string>("client_secret", _options.ClientSecret),
            new KeyValuePair<string, string>("redirect_uri", _options.RedirectUri),
            new KeyValuePair<string, string>("code", code),
        });

        var response = await _httpClient.PostAsync(
            $"{_options.AuthBaseUrl}/connect/token",
            tokenRequest,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content
            .ReadFromJsonAsync<TrueLayerTokenResponse>(
                cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException(
                "TrueLayer token endpoint returned null response.");

        var expiresAt = DateTimeOffset.UtcNow
            .AddSeconds(tokenResponse.ExpiresIn)
            .AddMinutes(-1); // small buffer to avoid using an expiring token

        return (tokenResponse.AccessToken, tokenResponse.RefreshToken, expiresAt);
    }

    public async Task<(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAt)>
        RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var tokenRequest = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("client_id", _options.ClientId),
            new KeyValuePair<string, string>("client_secret", _options.ClientSecret),
            new KeyValuePair<string, string>("refresh_token", refreshToken),
        });

        var response = await _httpClient.PostAsync(
            $"{_options.AuthBaseUrl}/connect/token",
            tokenRequest,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content
            .ReadFromJsonAsync<TrueLayerTokenResponse>(
                cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException(
                "TrueLayer token refresh returned null response.");

        var expiresAt = DateTimeOffset.UtcNow
            .AddSeconds(tokenResponse.ExpiresIn)
            .AddMinutes(-1);

        return (tokenResponse.AccessToken, tokenResponse.RefreshToken, expiresAt);
    }
}
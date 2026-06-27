using System.Text.Json.Serialization;

namespace FinTrack.Infrastructure.OpenBanking.Models;

/// <summary>
/// Matches TrueLayer's token endpoint response exactly.
/// Field names use snake_case to match the JSON directly —
/// System.Text.Json deserialises these without any custom converter.
/// </summary>
public class TrueLayerTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; } // seconds until access token expires
}
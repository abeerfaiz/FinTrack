namespace FinTrack.Infrastructure.OpenBanking;

/// <summary>
/// Strongly-typed configuration for TrueLayer API credentials and
/// endpoints. Bound from the "TrueLayer" section of configuration
/// via the options pattern — no magic strings scattered through code.
/// </summary>
public class TrueLayerOptions
{
    public const string SectionName = "TrueLayer";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string AuthBaseUrl { get; set; } = string.Empty;
    public string ApiBaseUrl { get; set; } = string.Empty;
    public string Scopes { get; set; } = string.Empty;
}
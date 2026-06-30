using System.Text.Json.Serialization;

namespace FinTrack.Infrastructure.OpenBanking.Models;

public class TrueLayerAccountListResponse
{
    [JsonPropertyName("results")]
    public List<TrueLayerAccount> Results { get; set; } = new();
}

public class TrueLayerAccount
{
    [JsonPropertyName("account_id")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("account_type")]
    public string AccountType { get; set; } = string.Empty;

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("account_number")]
    public TrueLayerAccountNumber? AccountNumber { get; set; }

    [JsonPropertyName("provider")]
    public TrueLayerProvider Provider { get; set; } = new();

    [JsonPropertyName("update_timestamp")]
    public DateTimeOffset UpdateTimestamp { get; set; }
}

public class TrueLayerAccountNumber
{
    [JsonPropertyName("iban")]
    public string? Iban { get; set; }

    [JsonPropertyName("number")]
    public string? Number { get; set; }

    [JsonPropertyName("sort_code")]
    public string? SortCode { get; set; }

    [JsonPropertyName("swift_bic")]
    public string? SwiftBic { get; set; }
}

public class TrueLayerProvider
{
    [JsonPropertyName("provider_id")]
    public string ProviderId { get; set; } = string.Empty;
}
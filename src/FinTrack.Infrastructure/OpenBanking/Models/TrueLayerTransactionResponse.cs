using System.Text.Json.Serialization;

namespace FinTrack.Infrastructure.OpenBanking.Models;

public class TrueLayerTransactionListResponse
{
    [JsonPropertyName("results")]
    public List<TrueLayerTransaction> Results { get; set; } = new();
}

public class TrueLayerTransaction
{
    [JsonPropertyName("transaction_id")]
    public string TransactionId { get; set; } = string.Empty;

    [JsonPropertyName("normalised_provider_transaction_id")]
    public string? NormalisedProviderTransactionId { get; set; }

    [JsonPropertyName("provider_transaction_id")]
    public string? ProviderTransactionId { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("transaction_type")]
    public string TransactionType { get; set; } = string.Empty;

    [JsonPropertyName("transaction_category")]
    public string TransactionCategory { get; set; } = string.Empty;

    [JsonPropertyName("transaction_classification")]
    public List<string> TransactionClassification { get; set; } = new();

    [JsonPropertyName("merchant_name")]
    public string? MerchantName { get; set; }

    [JsonPropertyName("running_balance")]
    public TrueLayerRunningBalance? RunningBalance { get; set; }
}

public class TrueLayerRunningBalance
{
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
}
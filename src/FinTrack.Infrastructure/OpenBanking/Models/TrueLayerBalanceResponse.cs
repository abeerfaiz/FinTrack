using System.Text.Json.Serialization;

namespace FinTrack.Infrastructure.OpenBanking.Models;

public class TrueLayerBalanceListResponse
{
    [JsonPropertyName("results")]
    public List<TrueLayerBalance> Results { get; set; } = new();
}

public class TrueLayerBalance
{
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("available")]
    public decimal Available { get; set; }

    [JsonPropertyName("current")]
    public decimal Current { get; set; }

    [JsonPropertyName("overdraft")]
    public decimal Overdraft { get; set; }
}
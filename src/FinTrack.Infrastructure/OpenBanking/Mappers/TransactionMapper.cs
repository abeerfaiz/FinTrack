using FinTrack.Application.Common.Interfaces;
using FinTrack.Infrastructure.OpenBanking.Models;
using System.Text.Json;

namespace FinTrack.Infrastructure.OpenBanking.Mappers;

public static class TransactionMapper
{
    /// <summary>
    /// Converts a raw TrueLayer transaction into the provider-agnostic
    /// OpenBankingTransaction DTO. The status is passed in separately
    /// because TrueLayer determines settled vs pending by which endpoint
    /// returned the result, not by a field within the transaction itself.
    /// </summary>
    public static OpenBankingTransaction ToOpenBankingTransaction(
        this TrueLayerTransaction source,
        string status)
    {
        return new OpenBankingTransaction(
            ExternalTxId: source.TransactionId,
            NormalisedProviderTxId: source.NormalisedProviderTransactionId,
            ProviderTransactionId: source.ProviderTransactionId,
            Status: status,
            TransactionType: source.TransactionType,
            TransactionCategory: source.TransactionCategory,
            TransactionClassification: source.TransactionClassification,
            Description: source.Description,
            MerchantName: source.MerchantName,
            Amount: source.Amount,
            Currency: source.Currency,
            TransactionDate: source.Timestamp,
            RunningBalance: source.RunningBalance?.Amount,
            RawPayloadJson: JsonSerializer.Serialize(source));
    }
}
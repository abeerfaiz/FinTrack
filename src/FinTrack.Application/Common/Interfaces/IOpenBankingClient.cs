namespace FinTrack.Application.Common.Interfaces;

/// <summary>
/// Abstraction over any Open Banking aggregator. Implemented by
/// TrueLayerClient in Infrastructure. If we ever needed to support
/// Plaid UK alongside or instead of TrueLayer, only Infrastructure
/// changes — every handler that depends on this interface is untouched.
/// </summary>
public interface IOpenBankingClient
{
    Task<OpenBankingTokenResult> ExchangeAuthCodeAsync(string authorisationCode, CancellationToken cancellationToken = default);

    Task<OpenBankingTokenResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OpenBankingAccount>> GetAccountsAsync(string accessToken, CancellationToken cancellationToken = default);

    Task<OpenBankingBalance> GetBalanceAsync(string accessToken, string accountId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OpenBankingTransaction>> GetTransactionsAsync(
        string accessToken,
        string accountId,
        DateTimeOffset? from = null,
        CancellationToken cancellationToken = default);

    Task<string> GetAuthorisationUrlAsync(string state, CancellationToken cancellationToken = default);
}

/// <summary>
/// Provider-agnostic DTOs. These shapes are deliberately generic —
/// not TrueLayer's exact JSON field names. The mapping from TrueLayer's
/// actual response shape into these happens entirely inside
/// Infrastructure's TrueLayerClient and its mappers. Application never
/// sees a TrueLayer-specific field name.
/// </summary>
public record OpenBankingTokenResult(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt);

public record OpenBankingAccount(
    string ExternalAccountId,
    string ProviderId,
    string AccountType,
    string DisplayName,
    string Currency,
    string? SortCode,
    string? AccountNumber,
    string? Iban,
    string? SwiftBic,
    DateTimeOffset UpdateTimestamp);

public record OpenBankingBalance(
    decimal Current,
    decimal Available,
    decimal Overdraft,
    string Currency);

public record OpenBankingTransaction(
    string ExternalTxId,
    string? NormalisedProviderTxId,
    string? ProviderTransactionId,
    string Status,
    string TransactionType,
    string TransactionCategory,
    IReadOnlyList<string> TransactionClassification,
    string Description,
    string? MerchantName,
    decimal Amount,
    string Currency,
    DateTimeOffset TransactionDate,
    decimal? RunningBalance,
    string RawPayloadJson);
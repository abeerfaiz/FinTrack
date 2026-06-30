using FinTrack.Application.Common.Interfaces;
using FinTrack.Infrastructure.OpenBanking.Models;

namespace FinTrack.Infrastructure.OpenBanking.Mappers;

/// <summary>
/// Converts TrueLayer's raw account shape into the provider-agnostic
/// OpenBankingAccount DTO that Application understands. This is the
/// only place in the codebase that knows TrueLayer's exact field names.
/// </summary>
public static class AccountMapper
{
    public static OpenBankingAccount ToOpenBankingAccount(this TrueLayerAccount source)
    {
        return new OpenBankingAccount(
            ExternalAccountId: source.AccountId,
            ProviderId: source.Provider.ProviderId,
            AccountType: source.AccountType,
            DisplayName: source.DisplayName,
            Currency: source.Currency,
            SortCode: source.AccountNumber?.SortCode,
            AccountNumber: source.AccountNumber?.Number,
            Iban: source.AccountNumber?.Iban,
            SwiftBic: source.AccountNumber?.SwiftBic,
            UpdateTimestamp: source.UpdateTimestamp);
    }
}
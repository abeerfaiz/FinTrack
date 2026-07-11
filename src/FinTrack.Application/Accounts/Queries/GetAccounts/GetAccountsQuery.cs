using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Accounts.Queries.GetAccounts;

public record GetAccountsQuery : IRequest<Result<IReadOnlyList<AccountDto>>>;

public record AccountDto(
    Guid Id,
    string ExternalAccountId,
    string DisplayName,
    string AccountType,
    string Currency,
    string? SortCode,
    string? AccountNumber,
    string? Iban,
    decimal? BalanceCurrent,
    decimal? BalanceAvailable,
    decimal? BalanceOverdraft,
    DateTimeOffset? BalanceUpdatedAt,
    DateTimeOffset? LastSyncedAt);
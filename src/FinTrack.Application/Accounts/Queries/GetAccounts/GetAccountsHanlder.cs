using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Accounts.Queries.GetAccounts;

public class GetAccountsHandler
    : IRequestHandler<GetAccountsQuery, Result<IReadOnlyList<AccountDto>>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetAccountsHandler(
        IAccountRepository accountRepository,
        ICurrentUserService currentUserService)
    {
        _accountRepository = accountRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<AccountDto>>> Handle(
        GetAccountsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        var accounts = await _accountRepository
            .GetByUserIdAsync(userId, cancellationToken);

        var dtos = accounts.Select(a => new AccountDto(
            Id: a.Id,
            ExternalAccountId: a.ExternalAccountId,
            DisplayName: a.DisplayName,
            AccountType: a.AccountType.ToString(),
            Currency: a.Currency,
            SortCode: a.SortCode,
            AccountNumber: a.AccountNumber,
            Iban: a.Iban,
            BalanceCurrent: a.BalanceCurrent,
            BalanceAvailable: a.BalanceAvailable,
            BalanceOverdraft: a.BalanceOverdraft,
            BalanceUpdatedAt: a.BalanceUpdatedAt,
            LastSyncedAt: a.LastSyncedAt))
            .ToList();

        return Result.Success<IReadOnlyList<AccountDto>>(dtos);
    }
}
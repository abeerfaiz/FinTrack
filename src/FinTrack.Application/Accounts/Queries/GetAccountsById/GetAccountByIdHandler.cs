using FinTrack.Application.Accounts.Queries.GetAccounts;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Application.Common.Models;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Exceptions;
using MediatR;

namespace FinTrack.Application.Accounts.Queries.GetAccountById;

public class GetAccountByIdHandler
    : IRequestHandler<GetAccountByIdQuery, Result<AccountDto>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetAccountByIdHandler(
        IAccountRepository accountRepository,
        ICurrentUserService currentUserService)
    {
        _accountRepository = accountRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AccountDto>> Handle(
        GetAccountByIdQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        var account = await _accountRepository
            .GetByIdAsync(request.AccountId, cancellationToken);

        if (account is null)
            throw new EntityNotFoundException(nameof(Account), request.AccountId);

        // IDOR protection
        if (account.UserId != userId)
            throw new UnauthorizedAccessException(
                "You do not have permission to view this account.");

        return Result.Success(new AccountDto(
            Id: account.Id,
            ExternalAccountId: account.ExternalAccountId,
            DisplayName: account.DisplayName,
            AccountType: account.AccountType.ToString(),
            Currency: account.Currency,
            SortCode: account.SortCode,
            AccountNumber: account.AccountNumber,
            Iban: account.Iban,
            BalanceCurrent: account.BalanceCurrent,
            BalanceAvailable: account.BalanceAvailable,
            BalanceOverdraft: account.BalanceOverdraft,
            BalanceUpdatedAt: account.BalanceUpdatedAt,
            LastSyncedAt: account.LastSyncedAt));
    }
}
using FinTrack.Application.Common.Models;
using FinTrack.Application.Accounts.Queries.GetAccounts;
using MediatR;

namespace FinTrack.Application.Accounts.Queries.GetAccountById;

public record GetAccountByIdQuery(Guid AccountId)
    : IRequest<Result<AccountDto>>;
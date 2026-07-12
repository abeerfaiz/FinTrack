using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.RecurringPayments.Queries.GetDirectDebits;

public class GetDirectDebitsHandler
    : IRequestHandler<GetDirectDebitsQuery, Result<IReadOnlyList<DirectDebitDto>>>
{
    private readonly IDirectDebitRepository _directDebitRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetDirectDebitsHandler(
        IDirectDebitRepository directDebitRepository,
        ICurrentUserService currentUserService)
    {
        _directDebitRepository = directDebitRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<DirectDebitDto>>> Handle(
        GetDirectDebitsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        var directDebits = await _directDebitRepository
            .GetByUserIdAsync(userId, cancellationToken);

        var dtos = directDebits.Select(d => new DirectDebitDto(
            Id: d.Id,
            AccountId: d.AccountId,
            Name: d.Name,
            Status: d.Status.ToString(),
            PreviousPaymentAmount: d.PreviousPaymentAmount,
            PreviousPaymentDate: d.PreviousPaymentDate,
            Currency: d.Currency,
            LastSyncedAt: d.LastSyncedAt))
            .ToList();

        return Result.Success<IReadOnlyList<DirectDebitDto>>(dtos);
    }
}
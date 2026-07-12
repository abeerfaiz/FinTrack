using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.RecurringPayments.Queries.GetStandingOrders;

public class GetStandingOrdersHandler
    : IRequestHandler<GetStandingOrdersQuery, Result<IReadOnlyList<StandingOrderDto>>>
{
    private readonly IStandingOrderRepository _standingOrderRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetStandingOrdersHandler(
        IStandingOrderRepository standingOrderRepository,
        ICurrentUserService currentUserService)
    {
        _standingOrderRepository = standingOrderRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<StandingOrderDto>>> Handle(
        GetStandingOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        var standingOrders = await _standingOrderRepository
            .GetByUserIdAsync(userId, cancellationToken);

        var dtos = standingOrders.Select(s => new StandingOrderDto(
            Id: s.Id,
            AccountId: s.AccountId,
            Status: s.Status.ToString(),
            Frequency: s.Frequency,
            Reference: s.Reference,
            Payee: s.Payee,
            Currency: s.Currency,
            NextPaymentDate: s.NextPaymentDate,
            NextPaymentAmount: s.NextPaymentAmount,
            FirstPaymentDate: s.FirstPaymentDate,
            FirstPaymentAmount: s.FirstPaymentAmount,
            FinalPaymentDate: s.FinalPaymentDate,
            FinalPaymentAmount: s.FinalPaymentAmount,
            LastSyncedAt: s.LastSyncedAt))
            .ToList();

        return Result.Success<IReadOnlyList<StandingOrderDto>>(dtos);
    }
}
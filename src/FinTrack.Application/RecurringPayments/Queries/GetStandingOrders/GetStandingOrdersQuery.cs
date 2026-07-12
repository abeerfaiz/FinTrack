using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.RecurringPayments.Queries.GetStandingOrders;

public record GetStandingOrdersQuery : IRequest<Result<IReadOnlyList<StandingOrderDto>>>;

public record StandingOrderDto(
    Guid Id,
    Guid AccountId,
    string Status,
    string Frequency,
    string? Reference,
    string? Payee,
    string Currency,
    DateTimeOffset? NextPaymentDate,
    decimal? NextPaymentAmount,
    DateTimeOffset? FirstPaymentDate,
    decimal? FirstPaymentAmount,
    DateTimeOffset? FinalPaymentDate,
    decimal? FinalPaymentAmount,
    DateTimeOffset LastSyncedAt);
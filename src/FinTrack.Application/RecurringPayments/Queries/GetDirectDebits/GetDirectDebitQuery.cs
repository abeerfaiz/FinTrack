using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.RecurringPayments.Queries.GetDirectDebits;

public record GetDirectDebitsQuery : IRequest<Result<IReadOnlyList<DirectDebitDto>>>;

public record DirectDebitDto(
    Guid Id,
    Guid AccountId,
    string Name,
    string Status,
    decimal PreviousPaymentAmount,
    DateTimeOffset PreviousPaymentDate,
    string Currency,
    DateTimeOffset LastSyncedAt);
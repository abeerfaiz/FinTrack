using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Transactions.Queries.GetMonthlySpending;

public record GetMonthlySpendingQuery(
    int Year,
    int Month) : IRequest<Result<IReadOnlyList<CategorySpendingDto>>>;

public record CategorySpendingDto(
    Guid CategoryId,
    string CategoryName,
    string CategoryColour,
    decimal TotalSpend,
    int TransactionCount);
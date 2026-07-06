using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Budgets.Queries.GetBudgetSummary;

public record GetBudgetSummaryQuery(DateOnly MonthStart)
    : IRequest<Result<IReadOnlyList<BudgetSummaryDto>>>;

public record BudgetSummaryDto(
    Guid BudgetId,
    Guid CategoryId,
    string CategoryName,
    string CategoryColour,
    decimal BudgetAmount,
    decimal ActualSpend,
    decimal Remaining,
    decimal PercentageUsed);
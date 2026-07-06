using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Transactions.Queries.GetSpendingTrend;

public record GetSpendingTrendQuery(int Months = 6)
    : IRequest<Result<IReadOnlyList<MonthlySpendDto>>>;

public record MonthlySpendDto(
    int Year,
    int Month,
    string MonthLabel,
    decimal TotalSpend);
using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Transactions.Queries.GetSpendingTrend;

public class GetSpendingTrendHandler
    : IRequestHandler<GetSpendingTrendQuery, Result<IReadOnlyList<MonthlySpendDto>>>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetSpendingTrendHandler(
        ITransactionRepository transactionRepository,
        ICurrentUserService currentUserService)
    {
        _transactionRepository = transactionRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<MonthlySpendDto>>> Handle(
        GetSpendingTrendQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        var months = Math.Clamp(request.Months, 1, 12);

        // Build date range covering the last N months
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var endMonth = new DateOnly(today.Year, today.Month, 1);
        var startMonth = endMonth.AddMonths(-(months - 1));

        var transactions = await _transactionRepository.GetByUserIdAsync(
            userId,
            from: startMonth,
            to: endMonth.AddMonths(1).AddDays(-1),
            cancellationToken: cancellationToken);

        // Group by year and month, sum debits only
        var spendByMonth = transactions
            .Where(t => t.Amount < 0)
            .GroupBy(t => new
            {
                t.TransactionDate.Year,
                t.TransactionDate.Month
            })
            .ToDictionary(
                g => (g.Key.Year, g.Key.Month),
                g => Math.Abs(g.Sum(t => t.Amount)));

        // Build result for every month in range — fill 0 for months
        // with no transactions so the chart has a complete timeline
        var result = new List<MonthlySpendDto>();
        var current = startMonth;

        while (current <= endMonth)
        {
            var spend = spendByMonth.GetValueOrDefault(
                (current.Year, current.Month), 0);

            result.Add(new MonthlySpendDto(
                Year: current.Year,
                Month: current.Month,
                MonthLabel: current.ToString("MMM yyyy"),
                TotalSpend: spend));

            current = current.AddMonths(1);
        }

        return Result.Success<IReadOnlyList<MonthlySpendDto>>(result);
    }
}
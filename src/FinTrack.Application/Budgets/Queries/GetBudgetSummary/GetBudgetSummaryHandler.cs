using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Application.Common.Models;
using FinTrack.Domain.Enums;
using MediatR;

namespace FinTrack.Application.Budgets.Queries.GetBudgetSummary;

public class GetBudgetSummaryHandler
    : IRequestHandler<GetBudgetSummaryQuery, Result<IReadOnlyList<BudgetSummaryDto>>>
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetBudgetSummaryHandler(
        IBudgetRepository budgetRepository,
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService)
    {
        _budgetRepository = budgetRepository;
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<BudgetSummaryDto>>> Handle(
        GetBudgetSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        var monthStart = new DateOnly(
            request.MonthStart.Year,
            request.MonthStart.Month,
            1);

        // Get all budgets for this user and month
        var budgets = await _budgetRepository
            .GetByUserAndMonthAsync(userId, monthStart, cancellationToken);

        if (!budgets.Any())
            return Result.Success<IReadOnlyList<BudgetSummaryDto>>(
                new List<BudgetSummaryDto>());

        // Get all settled transactions for this month
        // DateOnly range covers the entire month
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var transactions = await _transactionRepository.GetByUserIdAsync(
            userId,
            from: monthStart,
            to: monthEnd,
            cancellationToken: cancellationToken);

        // Only debits count as spending — credits are income
        // Amount is signed: negative = debit, positive = credit
        var spendByCategory = transactions
            .Where(t => t.Amount < 0 && t.UserCategoryId.HasValue)
            .GroupBy(t => t.UserCategoryId!.Value)
            .ToDictionary(
                g => g.Key,
                g => Math.Abs(g.Sum(t => t.Amount)));

        // Build summary for each budget
        var summaries = new List<BudgetSummaryDto>();

        foreach (var budget in budgets)
        {
            var category = await _categoryRepository
                .GetByIdAsync(budget.CategoryId, cancellationToken);

            if (category is null) continue;

            var actualSpend = spendByCategory.GetValueOrDefault(budget.CategoryId, 0);
            var remaining = budget.Amount - actualSpend;
            var percentageUsed = budget.Amount > 0
                ? Math.Round((actualSpend / budget.Amount) * 100, 1)
                : 0;

            summaries.Add(new BudgetSummaryDto(
                BudgetId: budget.Id,
                CategoryId: budget.CategoryId,
                CategoryName: category.Name,
                CategoryColour: category.ColourHex,
                BudgetAmount: budget.Amount,
                ActualSpend: actualSpend,
                Remaining: remaining,
                PercentageUsed: percentageUsed));
        }

        return Result.Success<IReadOnlyList<BudgetSummaryDto>>(summaries);
    }
}
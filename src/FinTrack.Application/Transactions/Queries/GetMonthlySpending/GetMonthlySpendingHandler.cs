using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Transactions.Queries.GetMonthlySpending;

public class GetMonthlySpendingHandler
    : IRequestHandler<GetMonthlySpendingQuery, Result<IReadOnlyList<CategorySpendingDto>>>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMonthlySpendingHandler(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<CategorySpendingDto>>> Handle(
        GetMonthlySpendingQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        var monthStart = new DateOnly(request.Year, request.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var transactions = await _transactionRepository.GetByUserIdAsync(
            userId,
            from: monthStart,
            to: monthEnd,
            cancellationToken: cancellationToken);

        // Only debits with a user-assigned category
        var categorised = transactions
            .Where(t => t.Amount < 0 && t.UserCategoryId.HasValue)
            .GroupBy(t => t.UserCategoryId!.Value)
            .ToList();

        var result = new List<CategorySpendingDto>();

        foreach (var group in categorised.OrderByDescending(g => Math.Abs(g.Sum(t => t.Amount))))
        {
            var category = await _categoryRepository
                .GetByIdAsync(group.Key, cancellationToken);

            if (category is null) continue;

            result.Add(new CategorySpendingDto(
                CategoryId: group.Key,
                CategoryName: category.Name,
                CategoryColour: category.ColourHex,
                TotalSpend: Math.Abs(group.Sum(t => t.Amount)),
                TransactionCount: group.Count()));
        }

        return Result.Success<IReadOnlyList<CategorySpendingDto>>(result);
    }
}
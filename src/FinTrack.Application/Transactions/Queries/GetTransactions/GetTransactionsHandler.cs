using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Transactions.Queries.GetTransactions;

public class GetTransactionsHandler
    : IRequestHandler<GetTransactionsQuery, Result<PagedResult<TransactionDto>>>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetTransactionsHandler(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<TransactionDto>>> Handle(
        GetTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        // Clamp page size — never allow a client to request 10,000 rows
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var page = Math.Max(1, request.Page);

        var (items, totalCount) = await _transactionRepository.GetPagedAsync(
            userId: userId,
            accountId: request.AccountId,
            categoryId: request.CategoryId,
            from: request.From,
            to: request.To,
            status: request.Status,
            page: page,
            pageSize: pageSize,
            cancellationToken: cancellationToken);

        // Build category lookup for the current page only
        // Not all transactions have a user category — only fetch
        // categories for the ones that do
        var categoryIds = items
            .Where(t => t.UserCategoryId.HasValue)
            .Select(t => t.UserCategoryId!.Value)
            .Distinct()
            .ToList();

        var categories = new Dictionary<Guid, (string Name, string Colour)>();
        foreach (var categoryId in categoryIds)
        {
            var category = await _categoryRepository
                .GetByIdAsync(categoryId, cancellationToken);
            if (category is not null)
                categories[categoryId] = (category.Name, category.ColourHex);
        }

        var dtos = items.Select(t =>
        {
            var categoryInfo = t.UserCategoryId.HasValue
                && categories.TryGetValue(t.UserCategoryId.Value, out var cat)
                ? cat
                : ((string Name, string Colour)?)null;

            return new TransactionDto(
                Id: t.Id,
                AccountId: t.AccountId,
                ExternalTxId: t.ExternalTxId,
                Status: t.Status.ToString(),
                TransactionType: t.TransactionType.ToString(),
                TransactionCategory: t.TransactionCategory,
                TransactionClassification: t.TransactionClassification,
                Description: t.Description,
                MerchantName: t.MerchantName,
                Amount: t.Amount,
                Currency: t.Currency,
                TransactionDate: t.TransactionDate,
                RunningBalance: t.RunningBalance,
                UserCategoryId: t.UserCategoryId,
                UserCategoryName: categoryInfo?.Name,
                UserCategoryColour: categoryInfo?.Colour,
                IsManuallyCategorised: t.IsManuallyCategorised);
        }).ToList();

        return Result.Success(new PagedResult<TransactionDto>(
            dtos, totalCount, page, pageSize));
    }
}
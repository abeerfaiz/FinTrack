using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Categories.Queries.GetCategoryRules;

public class GetCategoryRulesHandler
    : IRequestHandler<GetCategoryRulesQuery, Result<IReadOnlyList<CategoryRuleDto>>>
{
    private readonly ICategoryRuleRepository _categoryRuleRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetCategoryRulesHandler(
        ICategoryRuleRepository categoryRuleRepository,
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService)
    {
        _categoryRuleRepository = categoryRuleRepository;
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<CategoryRuleDto>>> Handle(
        GetCategoryRulesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        var rules = await _categoryRuleRepository
            .GetByUserIdAsync(userId, cancellationToken);

        var categories = await _categoryRepository
            .GetAvailableForUserAsync(userId, cancellationToken);

        var categoryNames = categories.ToDictionary(c => c.Id, c => c.Name);

        var dtos = rules
            .Select(r => new CategoryRuleDto(
                r.Id,
                r.CategoryId,
                categoryNames.TryGetValue(r.CategoryId, out var name) ? name : "Unknown",
                r.Keyword,
                r.Priority))
            .ToList();

        return Result.Success<IReadOnlyList<CategoryRuleDto>>(dtos);
    }
}

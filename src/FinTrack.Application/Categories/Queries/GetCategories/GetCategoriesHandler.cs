using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Categories.Queries.GetCategories;

public class GetCategoriesHandler
    : IRequestHandler<GetCategoriesQuery, Result<IReadOnlyList<CategoryDto>>>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetCategoriesHandler(
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService)
    {
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();

        var categories = await _categoryRepository
            .GetAvailableForUserAsync(userId, cancellationToken);

        var dtos = categories
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.ColourHex,
                c.Icon,
                c.IsSystem))
            .ToList();

        return Result.Success<IReadOnlyList<CategoryDto>>(dtos);
    }
}
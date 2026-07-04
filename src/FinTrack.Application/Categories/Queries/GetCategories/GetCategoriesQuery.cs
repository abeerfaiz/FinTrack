using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Categories.Queries.GetCategories;

public record GetCategoriesQuery : IRequest<Result<IReadOnlyList<CategoryDto>>>;

public record CategoryDto(
    Guid Id,
    string Name,
    string ColourHex,
    string Icon,
    bool IsSystem);
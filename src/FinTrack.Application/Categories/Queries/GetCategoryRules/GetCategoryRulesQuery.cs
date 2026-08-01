using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Categories.Queries.GetCategoryRules;

public record GetCategoryRulesQuery : IRequest<Result<IReadOnlyList<CategoryRuleDto>>>;

public record CategoryRuleDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Keyword,
    int Priority);

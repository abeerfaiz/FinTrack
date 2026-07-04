using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Categories.Commands.CreateCategory;

public record CreateCategoryRuleCommand(
    Guid CategoryId,
    string Keyword,
    int Priority) : IRequest<Result<Guid>>;
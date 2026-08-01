using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Categories.Commands.DeleteCategoryRule;

public record DeleteCategoryRuleCommand(Guid RuleId) : IRequest<Result>;

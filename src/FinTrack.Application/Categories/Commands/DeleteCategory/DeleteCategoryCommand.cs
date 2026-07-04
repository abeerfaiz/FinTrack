using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Categories.Commands.DeleteCategory;

public record DeleteCategoryCommand(Guid CategoryId) : IRequest<Result>;
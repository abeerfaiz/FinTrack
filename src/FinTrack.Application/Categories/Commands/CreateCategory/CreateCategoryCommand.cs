using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(
    string Name,
    string ColourHex,
    string Icon) : IRequest<Result<Guid>>;
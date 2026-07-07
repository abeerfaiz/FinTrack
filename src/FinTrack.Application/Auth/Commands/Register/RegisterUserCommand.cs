using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Auth.Commands.Register;

public record RegisterUserCommand(
    string Email,
    string Password,
    string DisplayName) : IRequest<Result<Guid>>;
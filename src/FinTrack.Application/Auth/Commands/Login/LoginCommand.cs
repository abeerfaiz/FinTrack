using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password) : IRequest<Result<LoginResult>>;

public record LoginResult(
    Guid UserId,
    string Email,
    string AccessToken);
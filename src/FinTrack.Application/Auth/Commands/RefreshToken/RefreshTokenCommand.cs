using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<RefreshTokenResult>>;

public record RefreshTokenResult(
    string AccessToken,
    string RefreshToken);
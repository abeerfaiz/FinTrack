using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.BankConnections.Commands.CompleteConnection;

/// <summary>
/// Receives the auth code from TrueLayer's callback redirect,
/// exchanges it for tokens, encrypts them, and persists the
/// bank connection. The state parameter is validated against
/// what was generated during initiation.
/// </summary>
public record CompleteConnectionCommand(
    string Code,
    string State) : IRequest<Result<Guid>>;
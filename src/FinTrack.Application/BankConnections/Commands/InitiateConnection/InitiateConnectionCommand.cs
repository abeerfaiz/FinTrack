using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.BankConnections.Commands.InitiateConnection;

/// <summary>
/// Builds a TrueLayer authorisation URL for the user to be redirected to.
/// Returns the URL plus a state token the client must echo back in the
/// callback — used to prevent CSRF attacks on the OAuth2 flow.
/// </summary>
public record InitiateConnectionCommand : IRequest<Result<InitiateConnectionResult>>;

public record InitiateConnectionResult(
    string AuthorisationUrl,
    string State);
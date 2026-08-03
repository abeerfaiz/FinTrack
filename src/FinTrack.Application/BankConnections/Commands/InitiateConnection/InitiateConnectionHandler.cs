using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Models;
using MediatR;
using System.Security.Cryptography;

namespace FinTrack.Application.BankConnections.Commands.InitiateConnection;

public class InitiateConnectionHandler
    : IRequestHandler<InitiateConnectionCommand, Result<InitiateConnectionResult>>
{
    private readonly IOpenBankingClient _openBankingClient;
    private readonly ICurrentUserService _currentUserService;
    private readonly IOAuthStateService _oAuthStateService;

    public InitiateConnectionHandler(
        IOpenBankingClient openBankingClient,
        ICurrentUserService currentUserService,
        IOAuthStateService oAuthStateService)
    {
        _openBankingClient = openBankingClient;
        _currentUserService = currentUserService;
        _oAuthStateService = oAuthStateService;
    }

    public async Task<Result<InitiateConnectionResult>> Handle(
        InitiateConnectionCommand request,
        CancellationToken cancellationToken)
    {
        // Get the real authenticated user's ID from JWT claim
        // No more hardcoded test user GUID
        var userId = _currentUserService.GetCurrentUserId();

        // Generate cryptographically random state component
        // State = userId:randomPart:hmac
        // Callback decodes userId from state to associate the
        // bank connection with the right user — no JWT in callback.
        // The HMAC signature (over userId:randomPart, using a server-only
        // key) is what makes state unforgeable — without it, anyone could
        // hand-craft a state naming another user's userId and hijack their
        // bank connection via the AllowAnonymous callback endpoint.
        var randomPart = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(16))
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        var statePayload = $"{userId}:{randomPart}";
        var signature = _oAuthStateService.Sign(statePayload);
        var state = $"{statePayload}:{signature}";

        var authorisationUrl = await _openBankingClient
            .GetAuthorisationUrlAsync(state, cancellationToken);

        return Result.Success(new InitiateConnectionResult(authorisationUrl, state));
    }
}
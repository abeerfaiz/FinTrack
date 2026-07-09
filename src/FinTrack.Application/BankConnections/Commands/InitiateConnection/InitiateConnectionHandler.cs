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

    public InitiateConnectionHandler(
        IOpenBankingClient openBankingClient,
        ICurrentUserService currentUserService)
    {
        _openBankingClient = openBankingClient;
        _currentUserService = currentUserService;
    }

    public async Task<Result<InitiateConnectionResult>> Handle(
        InitiateConnectionCommand request,
        CancellationToken cancellationToken)
    {
        // Get the real authenticated user's ID from JWT claim
        // No more hardcoded test user GUID
        var userId = _currentUserService.GetCurrentUserId();

        // Generate cryptographically random state component
        // State = userId:randomPart
        // Callback decodes userId from state to associate the
        // bank connection with the right user — no JWT in callback
        var randomPart = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(16))
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        var state = $"{userId}:{randomPart}";

        var authorisationUrl = await _openBankingClient
            .GetAuthorisationUrlAsync(state, cancellationToken);

        return Result.Success(new InitiateConnectionResult(authorisationUrl, state));
    }
}
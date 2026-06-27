using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Models;
using MediatR;

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
        // Temporary: use a fixed test user ID until auth is implemented.
        // In production, state encodes the authenticated user's ID
        // so the callback can associate the bank connection correctly
        // without requiring a JWT in the redirect.
        var testUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        var randomPart = Convert.ToBase64String(
            System.Security.Cryptography.RandomNumberGenerator.GetBytes(16))
            .Replace("+", "-").Replace("/", "_").Replace("=", "");

        // Encode userId into state: "userId:randomPart"
        var state = $"{testUserId}:{randomPart}";

        var authorisationUrl = await _openBankingClient
            .GetAuthorisationUrlAsync(state, cancellationToken);

        return Result.Success(new InitiateConnectionResult(authorisationUrl, state));
    }
}
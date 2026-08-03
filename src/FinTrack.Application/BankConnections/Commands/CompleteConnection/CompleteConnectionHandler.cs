using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Application.Common.Models;
using FinTrack.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinTrack.Application.BankConnections.Commands.CompleteConnection;

public class CompleteConnectionHandler
    : IRequestHandler<CompleteConnectionCommand, Result<Guid>>
{
    private readonly IOpenBankingClient _openBankingClient;
    private readonly ITokenEncryptionService _tokenEncryptionService;
    private readonly IBankConnectionRepository _bankConnectionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOAuthStateService _oAuthStateService;
    private readonly ILogger<CompleteConnectionHandler> _logger;

    public CompleteConnectionHandler(
        IOpenBankingClient openBankingClient,
        ITokenEncryptionService tokenEncryptionService,
        IBankConnectionRepository bankConnectionRepository,
        IUnitOfWork unitOfWork,
        IOAuthStateService oAuthStateService,
        ILogger<CompleteConnectionHandler> logger)
    {
        _openBankingClient = openBankingClient;
        _tokenEncryptionService = tokenEncryptionService;
        _bankConnectionRepository = bankConnectionRepository;
        _unitOfWork = unitOfWork;
        _oAuthStateService = oAuthStateService;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
    CompleteConnectionCommand request,
    CancellationToken cancellationToken)
    {
        // state = userId:randomPart:hmac
        // The HMAC is verified below before the userId is trusted —
        // without it, this AllowAnonymous endpoint would let anyone
        // hand-craft a state naming another user's userId.
        var stateParts = request.State.Split(':');
        if (stateParts.Length != 3 || !Guid.TryParse(stateParts[0], out var userId))
            return Result.Failure<Guid>("Invalid state parameter.");

        var statePayload = $"{stateParts[0]}:{stateParts[1]}";
        var providedSignature = stateParts[2];

        if (!_oAuthStateService.Verify(statePayload, providedSignature))
        {
            _logger.LogWarning("Bank connection callback rejected: state signature mismatch.");
            return Result.Failure<Guid>("Invalid state parameter.");
        }

        var tokenResult = await _openBankingClient
            .ExchangeAuthCodeAsync(request.Code, cancellationToken);

        var encryptedAccessToken = _tokenEncryptionService
            .Encrypt(tokenResult.AccessToken);

        var encryptedRefreshToken = _tokenEncryptionService
            .Encrypt(tokenResult.RefreshToken);

        var bankConnection = new BankConnection(
            userId: userId,
            providerId: "truelayer",
            accessTokenEncrypted: encryptedAccessToken,
            refreshTokenEncrypted: encryptedRefreshToken,
            tokenExpiresAt: tokenResult.ExpiresAt);

        await _bankConnectionRepository.AddAsync(bankConnection, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(bankConnection.Id);
    }
}
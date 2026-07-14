using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Application.Common.Models;
using FinTrack.Application.Transactions.Commands.SyncTransactions;
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
    //private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<CompleteConnectionHandler> _logger;

    public CompleteConnectionHandler(
        IOpenBankingClient openBankingClient,
        ITokenEncryptionService tokenEncryptionService,
        IBankConnectionRepository bankConnectionRepository,
        //ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<CompleteConnectionHandler> logger)
    {
        _openBankingClient = openBankingClient;
        _tokenEncryptionService = tokenEncryptionService;
        _bankConnectionRepository = bankConnectionRepository;
        //_currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(
    CompleteConnectionCommand request,
    CancellationToken cancellationToken)
    {
        // Temporary: extract userId from state parameter.
        // In production this comes from ICurrentUserService (JWT claim).
        var stateParts = request.State.Split(':');
        if (stateParts.Length < 2 || !Guid.TryParse(stateParts[0], out var userId))
            return Result.Failure<Guid>("Invalid state parameter.");

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

        // Pull accounts, balances and initial transactions immediately so the
        // user sees their connected accounts right after the OAuth redirect,
        // instead of waiting for the next scheduled sync.
        try
        {
            await _mediator.Send(
                new SyncTransactionsCommand(bankConnection.Id), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Initial sync failed for connection {ConnectionId}; connection was still created.",
                bankConnection.Id);
        }

        return Result.Success(bankConnection.Id);
    }
}
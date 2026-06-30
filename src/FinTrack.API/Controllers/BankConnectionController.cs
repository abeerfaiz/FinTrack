using FinTrack.Application.BankConnections.Commands.CompleteConnection;
using FinTrack.Application.BankConnections.Commands.InitiateConnection;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

[ApiController]
[Route("api/bank-connections")]
//[Authorize]
public class BankConnectionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BankConnectionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Returns a TrueLayer authorisation URL for the user to be
    /// redirected to. The client stores the returned state value
    /// and sends it back in the callback to prevent CSRF.
    /// </summary>
    [HttpGet("initiate")]
    public async Task<IActionResult> Initiate(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new InitiateConnectionCommand(),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// TrueLayer redirects here after the user authenticates with
    /// their bank. Exchanges the auth code for tokens and persists
    /// the bank connection. In a real flow the client would verify
    /// the state parameter matches what it stored from /initiate.
    /// </summary>
    [HttpGet("callback")]
    [AllowAnonymous] // TrueLayer redirect — no JWT in this request
    public async Task<IActionResult> Callback(
        [FromQuery] string code,
        [FromQuery] string state,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest("Missing authorisation code from TrueLayer.");

        var result = await _mediator.Send(
            new CompleteConnectionCommand(code, state),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        // In production this would redirect to your frontend with
        // a success indicator. For now return the connection ID.
        return Ok(new { bankConnectionId = result.Value });
    }

    /// <summary>
    /// TEMPORARY — diagnostic endpoint to verify TrueLayer data fetch.
    /// Remove before Week 4.
    /// </summary>
    [HttpGet("{connectionId}/test-fetch")]
    [AllowAnonymous]
    public async Task<IActionResult> TestFetch(
        Guid connectionId,
        [FromServices] IBankConnectionRepository bankConnectionRepository,
        [FromServices] IOpenBankingClient openBankingClient,
        [FromServices] ITokenEncryptionService tokenEncryptionService,
        CancellationToken cancellationToken)
    {
        var connection = await bankConnectionRepository
            .GetByIdAsync(connectionId, cancellationToken);

        if (connection is null)
            return NotFound("Bank connection not found.");

        // Decrypt the stored token to use for the API call
        var accessToken = tokenEncryptionService
            .Decrypt(connection.AccessTokenEncrypted);

        // Fetch accounts from TrueLayer sandbox
        var accounts = await openBankingClient
            .GetAccountsAsync(accessToken, cancellationToken);

        // Fetch transactions for the first account
        var transactions = new List<object>();
        if (accounts.Any())
        {
            var firstAccount = accounts.First();
            var txs = await openBankingClient
                .GetTransactionsAsync(
                    accessToken,
                    firstAccount.ExternalAccountId,
                    cancellationToken: cancellationToken);

            transactions = txs.Select(t => new
            {
                t.ExternalTxId,
                t.Description,
                t.Amount,
                t.Currency,
                t.TransactionType,
                t.Status,
                t.TransactionClassification,
                t.MerchantName
            }).Cast<object>().ToList();
        }

        return Ok(new
        {
            accountCount = accounts.Count,
            accounts = accounts.Select(a => new
            {
                a.ExternalAccountId,
                a.DisplayName,
                a.AccountType,
                a.Currency,
                a.SortCode,
                a.AccountNumber
            }),
            transactionCount = transactions.Count,
            transactions
        });
    }
}
using FinTrack.Application.Transactions.Commands.SyncTransactions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

[ApiController]
[Route("api/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Manually trigger a transaction sync for a specific bank connection.
    /// Useful for the "Sync now" button in the UI — the Hangfire recurring
    /// job handles automatic syncing every 6 hours.
    /// </summary>
    [HttpPost("sync/{bankConnectionId}")]
    [AllowAnonymous] // temporary until JWT auth is fully implemented
    public async Task<IActionResult> Sync(
        Guid bankConnectionId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SyncTransactionsCommand(bankConnectionId),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
using FinTrack.Application.Transactions.Commands.CategoriseTransaction;
using FinTrack.Application.Transactions.Commands.SyncTransactions;
using FinTrack.Application.Transactions.Queries.GetTransactions;
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
    /// Get paginated transactions for the authenticated user.
    /// Filterable by account, category, date range, and status.
    /// Ordered by transaction date descending — most recent first.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] Guid? accountId = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetTransactionsQuery(
                accountId, categoryId, from, to, status, page, pageSize),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Manually trigger a transaction sync for a bank connection.
    /// </summary>
    [HttpPost("sync/{bankConnectionId}")]
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

    /// <summary>
    /// Manually assign a category to a transaction.
    /// Sets is_manually_categorised = true — auto rules never overwrite.
    /// </summary>
    [HttpPatch("{transactionId}/categorise")]
    public async Task<IActionResult> Categorise(
        Guid transactionId,
        [FromBody] CategoriseTransactionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CategoriseTransactionCommand(transactionId, request.CategoryId),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }
}

public record CategoriseTransactionRequest(Guid CategoryId);
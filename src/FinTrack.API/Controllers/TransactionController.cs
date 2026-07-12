using FinTrack.Application.Transactions.Commands.CategoriseTransaction;
using FinTrack.Application.Transactions.Commands.SyncTransactions;
using FinTrack.Application.Transactions.Queries.GetTransactions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

/// <summary>
/// Bank transactions synced from connected accounts via Open Banking.
/// All endpoints require JWT authentication.
/// </summary>
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
    /// </summary>
    /// <remarks>
    /// All filters are optional and composable.
    /// Results are ordered by transaction date descending (most recent first).
    /// Settled and pending transactions are both returned unless filtered by status.
    /// Page size is clamped to a maximum of 100 to prevent large data pulls.
    /// </remarks>
    /// <param name="accountId">Filter by a specific bank account.</param>
    /// <param name="categoryId">Filter by a specific user category.</param>
    /// <param name="from">Start date filter (inclusive). Format: yyyy-MM-dd</param>
    /// <param name="to">End date filter (inclusive). Format: yyyy-MM-dd</param>
    /// <param name="status">Filter by status: Settled or Pending.</param>
    /// <param name="page">Page number (1-based). Defaults to 1.</param>
    /// <param name="pageSize">Items per page. Defaults to 20, max 100.</param>
    [HttpGet]
    [ProducesResponseType(typeof(FinTrack.Application.Common.Models.PagedResult<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    /// Manually trigger a transaction sync for a specific bank connection.
    /// </summary>
    /// <remarks>
    /// This is the on-demand sync endpoint. The Hangfire background job
    /// runs automatically every 6 hours — this endpoint is for immediate syncs.
    /// Tokens are refreshed proactively if expiring within 5 minutes.
    /// </remarks>
    /// <param name="bankConnectionId">The ID of the bank connection to sync.</param>
    [HttpPost("sync/{bankConnectionId}")]
    [ProducesResponseType(typeof(SyncTransactionsResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    /// </summary>
    /// <remarks>
    /// Sets is_manually_categorised = true on the transaction.
    /// Once manually categorised, auto-categorisation rules will
    /// never overwrite this choice — even on subsequent syncs.
    /// To re-enable auto-categorisation, categorise again with isManual = false
    /// (not yet exposed — coming in a future version).
    /// </remarks>
    /// <param name="transactionId">The transaction to categorise.</param>
    [HttpPatch("{transactionId}/categorise")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
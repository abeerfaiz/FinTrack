using FinTrack.Application.RecurringPayments.Queries.GetDirectDebits;
using FinTrack.Application.RecurringPayments.Queries.GetStandingOrders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

/// <summary>
/// Recurring payments — direct debits and standing orders.
/// Synced from connected bank accounts via Open Banking.
/// All endpoints require JWT authentication.
/// </summary>
[ApiController]
[Route("api/recurring-payments")]
[Authorize]
public class RecurringPaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RecurringPaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all direct debits for the authenticated user.
    /// </summary>
    /// <remarks>
    /// Ordered alphabetically by name.
    /// Returns empty array if no direct debits found —
    /// not all banks expose direct debit data via Open Banking.
    /// </remarks>
    [HttpGet("direct-debits")]
    [ProducesResponseType(typeof(IReadOnlyList<DirectDebitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDirectDebits(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetDirectDebitsQuery(), cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Get all standing orders for the authenticated user.
    /// </summary>
    /// <remarks>
    /// Ordered by next payment date ascending — soonest payment first.
    /// Returns empty array if no standing orders found.
    /// </remarks>
    [HttpGet("standing-orders")]
    [ProducesResponseType(typeof(IReadOnlyList<StandingOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStandingOrders(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetStandingOrdersQuery(), cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
using FinTrack.Application.RecurringPayments.Queries.GetDirectDebits;
using FinTrack.Application.RecurringPayments.Queries.GetStandingOrders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

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
    /// Ordered alphabetically by name.
    /// </summary>
    [HttpGet("direct-debits")]
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
    /// Ordered by next payment date ascending — soonest first.
    /// </summary>
    [HttpGet("standing-orders")]
    public async Task<IActionResult> GetStandingOrders(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetStandingOrdersQuery(), cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
using FinTrack.Application.Transactions.Queries.GetMonthlySpending;
using FinTrack.Application.Transactions.Queries.GetSpendingTrend;
using FinTrack.Application.Transactions.Queries.GetTopMerchants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

[ApiController]
[Route("api/spending")]
[AllowAnonymous] // temporary until Week 5
public class SpendingController : ControllerBase
{
    private readonly IMediator _mediator;

    public SpendingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Total spend broken down by category for a given month.
    /// Ordered by highest spend first — ready for a pie chart.
    /// </summary>
    [HttpGet("{year}/{month}/categories")]
    public async Task<IActionResult> GetMonthlySpending(
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        if (month < 1 || month > 12)
            return BadRequest("Month must be between 1 and 12.");

        var result = await _mediator.Send(
            new GetMonthlySpendingQuery(year, month),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Month-by-month total spend for the last N months.
    /// Zero-filled for months with no transactions — ready for a line chart.
    /// Defaults to last 6 months.
    /// </summary>
    [HttpGet("trend")]
    public async Task<IActionResult> GetSpendingTrend(
        [FromQuery] int months = 6,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetSpendingTrendQuery(months),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Top merchants by total spend for a given month.
    /// Falls back to transaction description when merchant name
    /// is not available — consistent with the rules engine.
    /// </summary>
    [HttpGet("{year}/{month}/top-merchants")]
    public async Task<IActionResult> GetTopMerchants(
        int year,
        int month,
        [FromQuery] int top = 5,
        CancellationToken cancellationToken = default)
    {
        if (month < 1 || month > 12)
            return BadRequest("Month must be between 1 and 12.");

        var result = await _mediator.Send(
            new GetTopMerchantsQuery(year, month, top),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
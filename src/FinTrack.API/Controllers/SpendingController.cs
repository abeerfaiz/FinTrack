using FinTrack.Application.Transactions.Queries.GetMonthlySpending;
using FinTrack.Application.Transactions.Queries.GetSpendingTrend;
using FinTrack.Application.Transactions.Queries.GetTopMerchants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

/// <summary>
/// Spending analytics — monthly breakdowns, trends, and top merchants.
/// All data is calculated from settled transactions only.
/// All endpoints require JWT authentication.
/// </summary>
[ApiController]
[Route("api/spending")]
[Authorize]
public class SpendingController : ControllerBase
{
    private readonly IMediator _mediator;

    public SpendingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Total spend broken down by category for a given month.
    /// </summary>
    /// <remarks>
    /// Only includes transactions with a user-assigned category.
    /// Uncategorised transactions are excluded.
    /// Results are ordered by highest spend first — ready for a pie chart.
    /// Debit transactions only (negative amounts).
    /// </remarks>
    /// <param name="year">The year (e.g. 2026).</param>
    /// <param name="month">The month number 1-12.</param>
    [HttpGet("{year}/{month}/categories")]
    [ProducesResponseType(typeof(IReadOnlyList<CategorySpendingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMonthlySpending(
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        if (month < 1 || month > 12)
            return BadRequest("Month must be between 1 and 12.");

        var result = await _mediator.Send(
            new GetMonthlySpendingQuery(year, month), cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Month-by-month total spend for the last N months.
    /// </summary>
    /// <remarks>
    /// Zero-filled for months with no transactions — every month in
    /// the range appears in the response even with £0 spend.
    /// This ensures the frontend always has a complete timeline for charts.
    /// Defaults to last 6 months.
    /// </remarks>
    /// <param name="months">Number of months to include (1-12). Defaults to 6.</param>
    [HttpGet("trend")]
    [ProducesResponseType(typeof(IReadOnlyList<MonthlySpendDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSpendingTrend(
        [FromQuery] int months = 6,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetSpendingTrendQuery(months), cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Top merchants by total spend for a given month.
    /// </summary>
    /// <remarks>
    /// Falls back to transaction description when merchant name is
    /// unavailable — consistent with the auto-categorisation rules engine.
    /// Debit transactions only.
    /// </remarks>
    /// <param name="year">The year (e.g. 2026).</param>
    /// <param name="month">The month number 1-12.</param>
    /// <param name="top">Number of merchants to return (1-20). Defaults to 5.</param>
    [HttpGet("{year}/{month}/top-merchants")]
    [ProducesResponseType(typeof(IReadOnlyList<MerchantSpendDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTopMerchants(
        int year,
        int month,
        [FromQuery] int top = 5,
        CancellationToken cancellationToken = default)
    {
        if (month < 1 || month > 12)
            return BadRequest("Month must be between 1 and 12.");

        var result = await _mediator.Send(
            new GetTopMerchantsQuery(year, month, top), cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
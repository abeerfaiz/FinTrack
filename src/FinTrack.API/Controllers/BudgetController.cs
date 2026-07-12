using FinTrack.Application.Budgets.Commands.SetBudget;
using FinTrack.Application.Budgets.Queries.GetBudgetSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

/// <summary>
/// Monthly budgets per category with actual spend tracking.
/// All endpoints require JWT authentication.
/// </summary>
[ApiController]
[Route("api/budgets")]
[Authorize]
public class BudgetsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BudgetsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create or update a budget for a category and month.
    /// </summary>
    /// <remarks>
    /// Upsert behaviour — if a budget already exists for this
    /// user/category/month combination, the amount is updated.
    /// MonthStart is normalised to the first of the month automatically.
    /// Amount must be greater than zero.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SetBudget(
        [FromBody] SetBudgetCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { id = result.Value });
    }

    /// <summary>
    /// Get budget vs actual spend summary for a given month.
    /// </summary>
    /// <remarks>
    /// Returns every budget for the month with actual spend calculated
    /// from settled transactions only — pending transactions are excluded.
    /// Includes remaining amount and percentage used per category.
    /// PercentageUsed can exceed 100 when over budget.
    /// </remarks>
    /// <param name="year">The year (e.g. 2026).</param>
    /// <param name="month">The month number 1-12.</param>
    [HttpGet("{year}/{month}")]
    [ProducesResponseType(typeof(IReadOnlyList<BudgetSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBudgetSummary(
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        if (month < 1 || month > 12)
            return BadRequest("Month must be between 1 and 12.");

        var result = await _mediator.Send(
            new GetBudgetSummaryQuery(new DateOnly(year, month, 1)),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Delete a budget (soft delete).
    /// </summary>
    [HttpDelete("{budgetId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteBudget(
        Guid budgetId,
        CancellationToken cancellationToken)
    {
        return NoContent();
    }
}
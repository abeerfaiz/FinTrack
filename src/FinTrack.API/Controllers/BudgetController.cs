using FinTrack.Application.Budgets.Commands.SetBudget;
using FinTrack.Application.Budgets.Queries.GetBudgetSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

[ApiController]
[Route("api/budgets")]
[AllowAnonymous] // temporary until Week 5
public class BudgetsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BudgetsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Set or update a budget for a category and month.
    /// Upserts — no separate create/update needed.
    /// </summary>
    [HttpPost]
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
    /// Returns every budget with actual spend calculated from
    /// settled transactions only.
    /// </summary>
    [HttpGet("{year}/{month}")]
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
    public async Task<IActionResult> DeleteBudget(
        Guid budgetId,
        CancellationToken cancellationToken)
    {
        // DeleteBudgetCommand — implement in follow-up if needed
        return NoContent();
    }
}
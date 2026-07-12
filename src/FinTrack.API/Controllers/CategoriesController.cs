using FinTrack.Application.Categories.Commands.CreateCategory;
using FinTrack.Application.Categories.Commands.DeleteCategory;
using FinTrack.Application.Categories.Queries.GetCategories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

/// <summary>
/// Transaction categories — system categories shared by all users,
/// plus user-defined custom categories.
/// All endpoints require JWT authentication.
/// </summary>
[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all categories available to the authenticated user.
    /// </summary>
    /// <remarks>
    /// Returns both system categories (shared, not deletable) and
    /// the user's own custom categories. System categories have IsSystem = true.
    /// Soft-deleted categories are automatically excluded.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Create a custom category for the authenticated user.
    /// </summary>
    /// <remarks>
    /// Custom categories are owned by the creating user and not
    /// visible to other users. ColourHex must be a valid 6-digit hex
    /// colour prefixed with # (e.g. #22C55E).
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateCategory(
        [FromBody] CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return CreatedAtAction(
            nameof(GetCategories),
            new { id = result.Value },
            new { id = result.Value });
    }

    /// <summary>
    /// Create a category rule for auto-categorisation.
    /// </summary>
    /// <remarks>
    /// Rules are matched against merchant name (or description when
    /// merchant name is unavailable) on every transaction sync.
    /// Lower priority number = higher priority (1 beats 10).
    /// First matching rule wins.
    /// </remarks>
    [HttpPost("rules")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateRule(
        [FromBody] CreateCategoryRuleCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { id = result.Value });
    }

    /// <summary>
    /// Soft-delete a user category.
    /// </summary>
    /// <remarks>
    /// System categories cannot be deleted and will return 422.
    /// Soft-deleted categories are hidden from all queries but
    /// data is preserved in the database.
    /// Transactions previously assigned this category retain the assignment.
    /// </remarks>
    [HttpDelete("{categoryId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeleteCategory(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new DeleteCategoryCommand(categoryId), cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return NoContent();
    }
}
using FinTrack.Application.Accounts.Queries.GetAccountById;
using FinTrack.Application.Accounts.Queries.GetAccounts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

/// <summary>
/// Bank accounts connected via Open Banking (TrueLayer).
/// All endpoints require JWT authentication.
/// </summary>
[ApiController]
[Route("api/accounts")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all connected bank accounts for the authenticated user.
    /// </summary>
    /// <remarks>
    /// Returns accounts with current and available balances
    /// as of the last sync. Balances update on every sync run.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AccountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAccounts(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAccountsQuery(), cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Get a single bank account by its FinTrack ID.
    /// </summary>
    /// <param name="accountId">The FinTrack account ID (not the bank's external ID).</param>
    [HttpGet("{accountId}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountById(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetAccountByIdQuery(accountId), cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }
}
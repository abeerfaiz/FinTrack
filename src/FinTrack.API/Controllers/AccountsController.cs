using FinTrack.Application.Accounts.Queries.GetAccountById;
using FinTrack.Application.Accounts.Queries.GetAccounts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

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
    /// Includes current and available balances updated on last sync.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAccounts(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetAccountsQuery(), cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// Get a single account by ID.
    /// Returns 404 if not found, 401 if it belongs to a different user.
    /// </summary>
    [HttpGet("{accountId}")]
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
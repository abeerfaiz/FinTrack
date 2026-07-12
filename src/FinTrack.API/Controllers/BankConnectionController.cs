using FinTrack.Application.BankConnections.Commands.CompleteConnection;
using FinTrack.Application.BankConnections.Commands.InitiateConnection;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

/// <summary>
/// Bank connections via TrueLayer Open Banking OAuth2 flow.
/// </summary>
[ApiController]
[Route("api/bank-connections")]
[Authorize]
public class BankConnectionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BankConnectionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Generate a TrueLayer authorisation URL to connect a bank account.
    /// </summary>
    /// <remarks>
    /// Returns an authorisationUrl to redirect the user's browser to.
    /// TrueLayer handles the bank authentication — FinTrack never
    /// sees the user's banking credentials.
    /// The state parameter must be stored by the client and verified
    /// on callback to prevent CSRF attacks.
    /// </remarks>
    [HttpGet("initiate")]
    [ProducesResponseType(typeof(InitiateConnectionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Initiate(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new InitiateConnectionCommand(), cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// TrueLayer OAuth2 callback — completes the bank connection.
    /// </summary>
    /// <remarks>
    /// This endpoint is called by TrueLayer after the user authenticates
    /// with their bank. The auth code is exchanged for access and refresh
    /// tokens server-side. Tokens are AES-256 encrypted before storage.
    /// This endpoint is AllowAnonymous because TrueLayer's redirect
    /// carries no JWT — user identity is recovered from the state parameter.
    /// </remarks>
    /// <param name="code">The authorisation code from TrueLayer.</param>
    /// <param name="state">The state parameter echoed from the initiate call.</param>
    [HttpGet("callback")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Callback(
        [FromQuery] string code,
        [FromQuery] string state,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest("Missing authorisation code from TrueLayer.");

        var result = await _mediator.Send(
            new CompleteConnectionCommand(code, state), cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(new { bankConnectionId = result.Value });
    }
}
using FinTrack.Application.BankConnections.Commands.CompleteConnection;
using FinTrack.Application.BankConnections.Commands.InitiateConnection;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinTrack.API.Controllers;

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
    /// Returns a TrueLayer authorisation URL for the user to be
    /// redirected to. The client stores the returned state value
    /// and sends it back in the callback to prevent CSRF.
    /// </summary>
    [HttpGet("initiate")]
    public async Task<IActionResult> Initiate(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new InitiateConnectionCommand(),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Value);
    }

    /// <summary>
    /// TrueLayer redirects here after the user authenticates with
    /// their bank. Exchanges the auth code for tokens and persists
    /// the bank connection. In a real flow the client would verify
    /// the state parameter matches what it stored from /initiate.
    /// </summary>
    [HttpGet("callback")]
    [AllowAnonymous] // TrueLayer redirect — no JWT in this request
    public async Task<IActionResult> Callback(
        [FromQuery] string code,
        [FromQuery] string state,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest("Missing authorisation code from TrueLayer.");

        var result = await _mediator.Send(
            new CompleteConnectionCommand(code, state),
            cancellationToken);

        if (result.IsFailure)
            return BadRequest(result.Error);

        // In production this would redirect to your frontend with
        // a success indicator. For now return the connection ID.
        return Ok(new { bankConnectionId = result.Value });
    }
}
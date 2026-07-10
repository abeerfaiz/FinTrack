using FinTrack.Application.BankConnections.Commands.CompleteConnection;
using FinTrack.Application.BankConnections.Commands.InitiateConnection;
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
    /// TrueLayer redirects here after user authenticates with their bank.
    /// AllowAnonymous because TrueLayer's redirect carries no JWT —
    /// user identity is recovered from the state parameter instead.
    /// </summary>
    [HttpGet("callback")]
    [AllowAnonymous]
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

        return Ok(new { bankConnectionId = result.Value });
    }
}
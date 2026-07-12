using FinTrack.Application.Auth.Commands.Login;
using FinTrack.Application.Auth.Commands.RefreshToken;
using FinTrack.Application.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinTrack.API.Controllers;

/// <summary>
/// Authentication — register, login, and token refresh.
/// All endpoints are public (no JWT required).
/// Rate limited to 5 requests per minute per IP.
/// </summary>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Register a new FinTrack account.
    /// </summary>
    /// <remarks>
    /// Password is hashed with BCrypt before storage.
    /// Email is normalised to lowercase.
    /// Returns 400 if email is already registered.
    /// </remarks>
    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { userId = result.Value });
    }

    /// <summary>
    /// Login and receive JWT access and refresh tokens.
    /// </summary>
    /// <remarks>
    /// Returns a generic error for both wrong email and wrong password
    /// to prevent user enumeration attacks.
    /// Access token expires after 24 hours.
    /// Refresh token expires after 7 days (single-use rotation).
    /// </remarks>
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(LoginResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Exchange a refresh token for a new access token and refresh token.
    /// </summary>
    /// <remarks>
    /// Refresh tokens are single-use — a new pair is issued on every call.
    /// The old refresh token is immediately invalidated.
    /// </remarks>
    [HttpPost("refresh")]
    [EnableRateLimiting("auth")]
    [ProducesResponseType(typeof(RefreshTokenResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
            return Unauthorized(new { error = result.Error });

        return Ok(result.Value);
    }
}
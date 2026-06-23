using FluentValidation;
using FinTrack.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace FinTrack.API.Middleware;

/// <summary>
/// Catches every unhandled exception from the request pipeline and
/// translates it into an appropriate HTTP response, so controllers
/// never need try/catch blocks. Each exception type maps to one
/// specific HTTP status code — this is where the exception/HTTP
/// status code contract for the whole API is defined in one place.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            // FluentValidation failures from ValidationBehaviour —
            // bad user input, 400 with field-level error detail.
            _logger.LogWarning("Validation failure: {Errors}",
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));

            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                title = "One or more validation errors occurred.",
                status = 400,
                errors
            }));
        }
        catch (EntityNotFoundException ex)
        {
            // Queried something that doesn't exist — 404.
            _logger.LogWarning(ex.Message);

            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                title = ex.Message,
                status = 404
            }));
        }
        catch (UnauthorizedAccessException ex)
        {
            // Missing or invalid JWT claim — 401.
            _logger.LogWarning(ex.Message);

            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                title = "Unauthorised.",
                status = 401
            }));
        }
        catch (DomainException ex)
        {
            // Domain invariant violated — caller did something that
            // should never have been possible. 422 Unprocessable Entity
            // rather than 400, because this isn't a validation failure —
            // the request was syntactically valid but semantically wrong
            // at the business rule level.
            _logger.LogWarning(ex.Message);

            context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                title = ex.Message,
                status = 422
            }));
        }
        catch (Exception ex)
        {
            // Truly unexpected — log the full exception with stack trace,
            // return 500, and never leak internal detail to the client.
            _logger.LogError(ex, "Unhandled exception for request {Path}", context.Request.Path);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                title = "An unexpected error occurred.",
                status = 500
            }));
        }
    }
}
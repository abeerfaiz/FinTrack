using FinTrack.Application.Common.Interfaces;
using System.Security.Claims;

namespace FinTrack.API.Services;

/// <summary>
/// Reads the authenticated user's ID from their JWT claim.
/// Lives in the API layer — it's the only layer that knows about
/// HttpContext. Application handlers call ICurrentUserService.GetCurrentUserId()
/// without knowing a JWT or HTTP request exists at all.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User
            .FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException(
                "No valid user identity found in the current request context. " +
                "Ensure the endpoint is protected with [Authorize].");

        return userId;
    }
}
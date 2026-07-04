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
        {
            // Temporary: return test user until JWT auth is implemented in Week 5
            return Guid.Parse("00000000-0000-0000-0000-000000000001");
        }

        return userId;
    }
}
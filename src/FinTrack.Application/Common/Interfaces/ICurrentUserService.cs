namespace FinTrack.Application.Common.Interfaces;

/// <summary>
/// Abstraction over "who is making this request". Implemented in the
/// API layer by reading the authenticated user's JWT claim — but
/// Application and Domain never know that. This is what makes every
/// "WHERE user_id = @userId" guard possible without handlers ever
/// touching HttpContext directly, which would be an API-layer concern
/// leaking into Application.
/// </summary>
public interface ICurrentUserService
{
    Guid GetCurrentUserId();
}
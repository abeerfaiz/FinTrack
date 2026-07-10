using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Application.Common.Models;
using MediatR;
using System.Security.Cryptography;
using System.Text;

namespace FinTrack.Application.Auth.Commands.RefreshToken;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResult>>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenHandler(
        IUserRepository userRepository,
        IJwtService jwtService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RefreshTokenResult>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // Hash the incoming token to look it up in the database.
        // We store SHA-256 hashes, not plaintext — if the database
        // is compromised, refresh tokens cannot be used directly.
        var tokenHash = HashToken(request.RefreshToken);

        var user = await _userRepository
            .GetByRefreshTokenHashAsync(tokenHash, cancellationToken);

        if (user is null)
            return Result.Failure<RefreshTokenResult>("Invalid refresh token.");

        // IsRefreshTokenValid checks the hash matches AND expiry is in future
        if (!user.IsRefreshTokenValid(request.RefreshToken))
            return Result.Failure<RefreshTokenResult>("Refresh token has expired.");

        // Rotate tokens — old refresh token is replaced with a new one.
        // Single-use pattern: if someone steals a refresh token and uses it,
        // the legitimate user's next refresh will fail, alerting them.
        var newAccessToken = _jwtService.GenerateToken(user.Id, user.Email);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.SetRefreshToken(HashToken(newRefreshToken), DateTimeOffset.UtcNow.AddDays(7));
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new RefreshTokenResult(newAccessToken, newRefreshToken));
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
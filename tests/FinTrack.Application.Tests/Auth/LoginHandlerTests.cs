using FinTrack.Application.Auth.Commands.Login;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Domain.Entities;
using FluentAssertions;
using Moq;

namespace FinTrack.Application.Tests.Auth;

public class LoginHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo;
    private readonly Mock<IJwtService> _jwtService;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _userRepo = new Mock<IUserRepository>();
        _jwtService = new Mock<IJwtService>();
        _unitOfWork = new Mock<IUnitOfWork>();

        _jwtService.Setup(j => j.GenerateToken(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns("fake-jwt-token");

        _jwtService.Setup(j => j.GenerateRefreshToken())
            .Returns("fake-refresh-token");

        _handler = new LoginHandler(
            _userRepo.Object,
            _jwtService.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokens()
    {
        // Arrange — create user with real BCrypt hash
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");
        var user = new User("abeer@fintrack.com", passwordHash, "Abeer");

        _userRepo.Setup(r => r.GetByEmailAsync("abeer@fintrack.com", default))
            .ReturnsAsync(user);

        var command = new LoginCommand("abeer@fintrack.com", "Password123!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("fake-jwt-token");
        result.Value.RefreshToken.Should().Be("fake-refresh-token");
        result.Value.Email.Should().Be("abeer@fintrack.com");
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsGenericFailure()
    {
        // Arrange — email doesn't exist
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync((User?)null);

        var command = new LoginCommand("notfound@fintrack.com", "Password123!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — generic message prevents user enumeration
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid email or password.");

        // Verify no token was generated
        _jwtService.Verify(j => j.GenerateToken(
            It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WrongPassword_ReturnsGenericFailure()
    {
        // Arrange — user exists but password is wrong
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!");
        var user = new User("abeer@fintrack.com", passwordHash, "Abeer");

        _userRepo.Setup(r => r.GetByEmailAsync("abeer@fintrack.com", default))
            .ReturnsAsync(user);

        var command = new LoginCommand("abeer@fintrack.com", "WrongPassword!");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — same generic message as user not found
        // This is deliberate: attacker cannot distinguish between
        // "email doesn't exist" and "password is wrong"
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Invalid email or password.");
        _jwtService.Verify(j => j.GenerateToken(
            It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SuccessfulLogin_SavesRefreshTokenHash()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");
        var user = new User("abeer@fintrack.com", passwordHash, "Abeer");

        _userRepo.Setup(r => r.GetByEmailAsync("abeer@fintrack.com", default))
            .ReturnsAsync(user);

        var command = new LoginCommand("abeer@fintrack.com", "Password123!");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — refresh token stored as hash, not plaintext
        user.RefreshToken.Should().NotBeNull();
        user.RefreshToken.Should().NotBe("fake-refresh-token"); // not plaintext
        user.RefreshTokenExpiresAt.Should().NotBeNull();
        user.RefreshTokenExpiresAt.Should()
            .BeCloseTo(DateTimeOffset.UtcNow.AddDays(7), TimeSpan.FromMinutes(1));
        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_SuccessfulLogin_IssuesJwtWithCorrectClaims()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");
        var user = new User("abeer@fintrack.com", passwordHash, "Abeer");

        _userRepo.Setup(r => r.GetByEmailAsync("abeer@fintrack.com", default))
            .ReturnsAsync(user);

        var command = new LoginCommand("abeer@fintrack.com", "Password123!");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — JWT generated with correct userId and email
        _jwtService.Verify(j => j.GenerateToken(user.Id, user.Email), Times.Once);
    }
}
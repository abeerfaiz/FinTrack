using FinTrack.Application.Auth.Commands.Register;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Domain.Entities;
using FluentAssertions;
using Moq;

namespace FinTrack.Application.Tests.Auth;

public class RegisterUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly RegisterUserHandler _handler;

    public RegisterUserHandlerTests()
    {
        _userRepo = new Mock<IUserRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();

        _handler = new RegisterUserHandler(
            _userRepo.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesUserSuccessfully()
    {
        // Arrange — email not already taken
        _userRepo.Setup(r => r.GetByEmailAsync("abeer@fintrack.com", default))
            .ReturnsAsync((User?)null);

        var command = new RegisterUserCommand(
            "abeer@fintrack.com",
            "Password123!",
            "Abeer");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        // Verify user was added and saved
        _userRepo.Verify(r => r.AddAsync(
            It.Is<User>(u => u.Email == "abeer@fintrack.com"),
            default), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ReturnsFailure()
    {
        // Arrange — email already exists
        var existingUser = new User(
            "abeer@fintrack.com",
            "hashedpassword",
            "Abeer");

        _userRepo.Setup(r => r.GetByEmailAsync("abeer@fintrack.com", default))
            .ReturnsAsync(existingUser);

        var command = new RegisterUserCommand(
            "abeer@fintrack.com",
            "Password123!",
            "Abeer");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already exists");

        // Verify nothing was saved
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), default), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_PasswordIsHashed_NeverStoredAsPlaintext()
    {
        // Arrange
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync((User?)null);

        const string plainPassword = "Password123!";
        var command = new RegisterUserCommand(
            "abeer@fintrack.com",
            plainPassword,
            "Abeer");

        User? capturedUser = null;
        _userRepo.Setup(r => r.AddAsync(It.IsAny<User>(), default))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — password hash must not equal the plaintext password
        capturedUser.Should().NotBeNull();
        capturedUser!.PasswordHash.Should().NotBe(plainPassword);
        capturedUser.PasswordHash.Should().StartWith("$2a$");
    }

    [Fact]
    public async Task Handle_EmailNormalisedToLowercase()
    {
        // Arrange — email with mixed case
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), default))
            .ReturnsAsync((User?)null);

        User? capturedUser = null;
        _userRepo.Setup(r => r.AddAsync(It.IsAny<User>(), default))
            .Callback<User, CancellationToken>((user, _) => capturedUser = user);

        var command = new RegisterUserCommand(
            "ABEER@FINTRACK.COM",
            "Password123!",
            "Abeer");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — email stored as lowercase
        capturedUser!.Email.Should().Be("abeer@fintrack.com");
    }
}
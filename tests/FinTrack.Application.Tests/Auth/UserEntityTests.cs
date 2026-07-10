using FinTrack.Domain.Entities;
using FinTrack.Domain.Exceptions;
using FluentAssertions;

namespace FinTrack.Application.Tests.Auth;

public class UserEntityTests
{
    [Fact]
    public void Constructor_ValidInputs_CreatesUser()
    {
        // Act
        var user = new User("abeer@fintrack.com", "hashedpassword", "Abeer");

        // Assert
        user.Id.Should().NotBeEmpty();
        user.Email.Should().Be("abeer@fintrack.com");
        user.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Constructor_EmptyEmail_ThrowsDomainException()
    {
        var act = () => new User("", "hashedpassword", "Abeer");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_EmptyPasswordHash_ThrowsDomainException()
    {
        var act = () => new User("abeer@fintrack.com", "", "Abeer");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void IsRefreshTokenValid_ValidToken_ReturnsTrue()
    {
        // Arrange
        var user = new User("abeer@fintrack.com", "hash", "Abeer");
        var token = "my-refresh-token";
        var tokenHash = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(token)));

        user.SetRefreshToken(tokenHash, DateTimeOffset.UtcNow.AddDays(7));

        // Act
        var isValid = user.IsRefreshTokenValid(token);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsRefreshTokenValid_ExpiredToken_ReturnsFalse()
    {
        // Arrange
        var user = new User("abeer@fintrack.com", "hash", "Abeer");
        var token = "my-refresh-token";
        var tokenHash = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(token)));

        // Set expiry in the past
        user.SetRefreshToken(tokenHash, DateTimeOffset.UtcNow.AddDays(-1));

        // Act
        var isValid = user.IsRefreshTokenValid(token);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void IsRefreshTokenValid_WrongToken_ReturnsFalse()
    {
        // Arrange
        var user = new User("abeer@fintrack.com", "hash", "Abeer");
        var correctToken = "correct-token";
        var tokenHash = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(correctToken)));

        user.SetRefreshToken(tokenHash, DateTimeOffset.UtcNow.AddDays(7));

        // Act — try wrong token
        var isValid = user.IsRefreshTokenValid("wrong-token");

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void RevokeRefreshToken_ClearsToken()
    {
        // Arrange
        var user = new User("abeer@fintrack.com", "hash", "Abeer");
        user.SetRefreshToken("some-hash", DateTimeOffset.UtcNow.AddDays(7));

        // Act
        user.RevokeRefreshToken();

        // Assert
        user.RefreshToken.Should().BeNull();
        user.RefreshTokenExpiresAt.Should().BeNull();
    }

    [Fact]
    public void UpdateDisplayName_ValidName_UpdatesName()
    {
        // Arrange
        var user = new User("abeer@fintrack.com", "hash", "Abeer");

        // Act
        user.UpdateDisplayName("Abeer Farid");

        // Assert
        user.DisplayName.Should().Be("Abeer Farid");
        user.UpdatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void UpdateDisplayName_EmptyName_ThrowsDomainException()
    {
        var user = new User("abeer@fintrack.com", "hash", "Abeer");
        var act = () => user.UpdateDisplayName("");
        act.Should().Throw<DomainException>();
    }
}
using FinTrack.Domain.Entities;
using FinTrack.Domain.Exceptions;
using FluentAssertions;

namespace FinTrack.Application.Tests.Categories;

public class CategoryEntityTests
{
    [Fact]
    public void Delete_SystemCategory_ThrowsDomainException()
    {
        // Arrange
        var category = TestHelpers.CreateSystemCategory();

        // Act
        var act = () => category.Delete();

        // Assert — system categories cannot be deleted by users
        act.Should().Throw<DomainException>()
            .WithMessage("*System categories cannot be deleted*");
    }

    [Fact]
    public void Delete_UserCategory_SetsDeletedAt()
    {
        // Arrange
        var category = TestHelpers.CreateUserCategory();

        // Act
        category.Delete();

        // Assert — soft delete
        category.IsDeleted.Should().BeTrue();
        category.DeletedAt.Should().NotBeNull();
        category.DeletedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Rename_SystemCategory_ThrowsDomainException()
    {
        // Arrange
        var category = TestHelpers.CreateSystemCategory();

        // Act
        var act = () => category.Rename("New Name");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*System categories cannot be renamed*");
    }

    [Fact]
    public void Rename_UserCategory_UpdatesName()
    {
        // Arrange
        var category = TestHelpers.CreateUserCategory(name: "Old Name");

        // Act
        category.Rename("New Name");

        // Assert
        category.Name.Should().Be("New Name");
    }

    [Fact]
    public void CreateSystemCategory_SetsCorrectDefaults()
    {
        // Act
        var category = Category.CreateSystemCategory("Groceries", "#22C55E", "shopping-cart");

        // Assert
        category.IsSystem.Should().BeTrue();
        category.UserId.Should().BeNull();
        category.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void CreateUserCategory_EmptyUserId_ThrowsDomainException()
    {
        // Act
        var act = () => Category.CreateUserCategory(
            Guid.Empty, "Test", "#22C55E", "star");

        // Assert
        act.Should().Throw<DomainException>();
    }
}
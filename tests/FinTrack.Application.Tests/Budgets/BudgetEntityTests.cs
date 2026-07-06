using FinTrack.Domain.Entities;
using FinTrack.Domain.Exceptions;
using FluentAssertions;

namespace FinTrack.Application.Tests.Budgets;

public class BudgetEntityTests
{
    [Fact]
    public void Constructor_NormalisesMonthStartToFirstOfMonth()
    {
        // Arrange & Act — passing 15th July
        var budget = TestHelpers.CreateBudget(
            monthStart: new DateOnly(2026, 7, 15));

        // Assert — stored as 1st July
        budget.MonthStart.Should().Be(new DateOnly(2026, 7, 1));
    }

    [Fact]
    public void Constructor_ZeroAmount_ThrowsDomainException()
    {
        // Act
        var act = () => new Budget(
            userId: Guid.NewGuid(),
            categoryId: Guid.NewGuid(),
            amount: 0m,
            monthStart: new DateOnly(2026, 7, 1));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*greater than zero*");
    }

    [Fact]
    public void Constructor_NegativeAmount_ThrowsDomainException()
    {
        // Act
        var act = () => new Budget(
            userId: Guid.NewGuid(),
            categoryId: Guid.NewGuid(),
            amount: -100m,
            monthStart: new DateOnly(2026, 7, 1));

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*greater than zero*");
    }

    [Fact]
    public void UpdateAmount_ValidAmount_UpdatesSuccessfully()
    {
        // Arrange
        var budget = TestHelpers.CreateBudget(amount: 300m);

        // Act
        budget.UpdateAmount(500m);

        // Assert
        budget.Amount.Should().Be(500m);
    }

    [Fact]
    public void UpdateAmount_ZeroAmount_ThrowsDomainException()
    {
        // Arrange
        var budget = TestHelpers.CreateBudget(amount: 300m);

        // Act
        var act = () => budget.UpdateAmount(0m);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Delete_SoftDeletesBudget()
    {
        // Arrange
        var budget = TestHelpers.CreateBudget();

        // Act
        budget.Delete();

        // Assert
        budget.IsDeleted.Should().BeTrue();
        budget.DeletedAt.Should().NotBeNull();
    }
}
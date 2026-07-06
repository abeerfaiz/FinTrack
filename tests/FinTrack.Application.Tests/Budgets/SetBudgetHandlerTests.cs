using FinTrack.Application.Budgets.Commands.SetBudget;
using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Domain.Entities;
using FluentAssertions;
using Moq;

namespace FinTrack.Application.Tests.Budgets;

public class SetBudgetHandlerTests
{
    private readonly Mock<IBudgetRepository> _budgetRepo;
    private readonly Mock<ICategoryRepository> _categoryRepo;
    private readonly Mock<ICurrentUserService> _currentUserService;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly SetBudgetHandler _handler;

    public SetBudgetHandlerTests()
    {
        _budgetRepo = new Mock<IBudgetRepository>();
        _categoryRepo = new Mock<ICategoryRepository>();
        _currentUserService = new Mock<ICurrentUserService>();
        _unitOfWork = new Mock<IUnitOfWork>();

        _currentUserService.Setup(s => s.GetCurrentUserId())
            .Returns(TestHelpers.TestUserId);

        _handler = new SetBudgetHandler(
            _budgetRepo.Object,
            _categoryRepo.Object,
            _currentUserService.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_NewBudget_CreatesBudgetSuccessfully()
    {
        // Arrange
        var category = TestHelpers.CreateSystemCategory();

        _categoryRepo.Setup(r => r.GetByIdAsync(category.Id, default))
            .ReturnsAsync(category);

        _budgetRepo.Setup(r => r.GetByUserCategoryMonthAsync(
            TestHelpers.TestUserId, category.Id, It.IsAny<DateOnly>(), default))
            .ReturnsAsync((Budget?)null);

        var command = new SetBudgetCommand(category.Id, 300m, new DateOnly(2026, 7, 1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _budgetRepo.Verify(r => r.AddAsync(It.IsAny<Budget>(), default), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingBudget_UpdatesAmountInsteadOfCreating()
    {
        // Arrange — budget already exists for this month/category
        var category = TestHelpers.CreateSystemCategory();
        var existingBudget = TestHelpers.CreateBudget(
            userId: TestHelpers.TestUserId,
            categoryId: category.Id,
            amount: 300m);

        _categoryRepo.Setup(r => r.GetByIdAsync(category.Id, default))
            .ReturnsAsync(category);

        _budgetRepo.Setup(r => r.GetByUserCategoryMonthAsync(
            TestHelpers.TestUserId, category.Id, It.IsAny<DateOnly>(), default))
            .ReturnsAsync(existingBudget);

        var command = new SetBudgetCommand(category.Id, 500m, new DateOnly(2026, 7, 15));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — updated, not created
        result.IsSuccess.Should().BeTrue();
        existingBudget.Amount.Should().Be(500m);
        _budgetRepo.Verify(r => r.AddAsync(It.IsAny<Budget>(), default), Times.Never);
        _budgetRepo.Verify(r => r.Update(existingBudget), Times.Once);
    }

    [Fact]
    public async Task Handle_MonthStartNormalised_AlwaysFirstOfMonth()
    {
        // Arrange — passing 15th of month should be normalised to 1st
        var category = TestHelpers.CreateSystemCategory();

        _categoryRepo.Setup(r => r.GetByIdAsync(category.Id, default))
            .ReturnsAsync(category);

        _budgetRepo.Setup(r => r.GetByUserCategoryMonthAsync(
            TestHelpers.TestUserId, category.Id,
            new DateOnly(2026, 7, 1), // normalised date
            default))
            .ReturnsAsync((Budget?)null);

        // Act — passing 15th July
        var command = new SetBudgetCommand(category.Id, 300m, new DateOnly(2026, 7, 15));
        await _handler.Handle(command, CancellationToken.None);

        // Assert — repository was called with 1st July, not 15th
        _budgetRepo.Verify(r => r.GetByUserCategoryMonthAsync(
            TestHelpers.TestUserId,
            category.Id,
            new DateOnly(2026, 7, 1),
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ReturnsFailure()
    {
        // Arrange
        _categoryRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Category?)null);

        var command = new SetBudgetCommand(Guid.NewGuid(), 300m, new DateOnly(2026, 7, 1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
        _budgetRepo.Verify(r => r.AddAsync(It.IsAny<Budget>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_CategoryBelongsToDifferentUser_ReturnsFailure()
    {
        // Arrange
        var category = TestHelpers.CreateUserCategory(userId: TestHelpers.OtherUserId);

        _categoryRepo.Setup(r => r.GetByIdAsync(category.Id, default))
            .ReturnsAsync(category);

        var command = new SetBudgetCommand(category.Id, 300m, new DateOnly(2026, 7, 1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        _budgetRepo.Verify(r => r.AddAsync(It.IsAny<Budget>(), default), Times.Never);
    }
}
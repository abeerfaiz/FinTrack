using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Application.Transactions.Commands.CategoriseTransaction;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace FinTrack.Application.Tests.Transactions;

public class CategoriseTransactionHandlerTests
{
    private readonly Mock<ITransactionRepository> _transactionRepo;
    private readonly Mock<ICategoryRepository> _categoryRepo;
    private readonly Mock<ICurrentUserService> _currentUserService;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly CategoriseTransactionHandler _handler;

    public CategoriseTransactionHandlerTests()
    {
        _transactionRepo = new Mock<ITransactionRepository>();
        _categoryRepo = new Mock<ICategoryRepository>();
        _currentUserService = new Mock<ICurrentUserService>();
        _unitOfWork = new Mock<IUnitOfWork>();

        _currentUserService.Setup(s => s.GetCurrentUserId())
            .Returns(TestHelpers.TestUserId);

        _handler = new CategoriseTransactionHandler(
            _transactionRepo.Object,
            _categoryRepo.Object,
            _currentUserService.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_SetsManualCategorisation()
    {
        // Arrange
        var transaction = TestHelpers.CreateTransaction(userId: TestHelpers.TestUserId);
        var category = TestHelpers.CreateSystemCategory();

        _transactionRepo.Setup(r => r.GetByIdAsync(transaction.Id, default))
            .ReturnsAsync(transaction);

        _categoryRepo.Setup(r => r.GetByIdAsync(category.Id, default))
            .ReturnsAsync(category);

        var command = new CategoriseTransactionCommand(transaction.Id, category.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        transaction.UserCategoryId.Should().Be(category.Id);
        transaction.IsManuallyCategorised.Should().BeTrue();
        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task Handle_TransactionNotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        _transactionRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Transaction?)null);

        var command = new CategoriseTransactionCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_TransactionBelongsToDifferentUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange — transaction belongs to a different user
        var transaction = TestHelpers.CreateTransaction(userId: TestHelpers.OtherUserId);

        _transactionRepo.Setup(r => r.GetByIdAsync(transaction.Id, default))
            .ReturnsAsync(transaction);

        var command = new CategoriseTransactionCommand(transaction.Id, Guid.NewGuid());

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert — IDOR protection working
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _unitOfWork.Verify(u => u.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ThrowsEntityNotFoundException()
    {
        // Arrange
        var transaction = TestHelpers.CreateTransaction(userId: TestHelpers.TestUserId);

        _transactionRepo.Setup(r => r.GetByIdAsync(transaction.Id, default))
            .ReturnsAsync(transaction);

        _categoryRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Category?)null);

        var command = new CategoriseTransactionCommand(transaction.Id, Guid.NewGuid());

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_CategoryBelongsToDifferentUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange — category belongs to a different user (not a system category)
        var transaction = TestHelpers.CreateTransaction(userId: TestHelpers.TestUserId);
        var category = TestHelpers.CreateUserCategory(userId: TestHelpers.OtherUserId);

        _transactionRepo.Setup(r => r.GetByIdAsync(transaction.Id, default))
            .ReturnsAsync(transaction);

        _categoryRepo.Setup(r => r.GetByIdAsync(category.Id, default))
            .ReturnsAsync(category);

        var command = new CategoriseTransactionCommand(transaction.Id, category.Id);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert — cannot use another user's custom category
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_SystemCategory_SucceedsForAnyUser()
    {
        // Arrange — system categories (user_id null) are accessible to everyone
        var transaction = TestHelpers.CreateTransaction(userId: TestHelpers.TestUserId);
        var systemCategory = TestHelpers.CreateSystemCategory();

        _transactionRepo.Setup(r => r.GetByIdAsync(transaction.Id, default))
            .ReturnsAsync(transaction);

        _categoryRepo.Setup(r => r.GetByIdAsync(systemCategory.Id, default))
            .ReturnsAsync(systemCategory);

        var command = new CategoriseTransactionCommand(transaction.Id, systemCategory.Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        transaction.IsManuallyCategorised.Should().BeTrue();
    }
}
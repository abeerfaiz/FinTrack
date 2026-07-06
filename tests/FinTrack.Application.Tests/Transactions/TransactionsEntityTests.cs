using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using FinTrack.Domain.Exceptions;
using FluentAssertions;

namespace FinTrack.Application.Tests.Transactions;

public class TransactionEntityTests
{
    [Fact]
    public void AssignCategory_AutoCategorisation_SetsCategory()
    {
        // Arrange
        var transaction = TestHelpers.CreateTransaction();
        var categoryId = Guid.NewGuid();

        // Act
        transaction.AssignCategory(categoryId, isManual: false);

        // Assert
        transaction.UserCategoryId.Should().Be(categoryId);
        transaction.IsManuallyCategorised.Should().BeFalse();
    }

    [Fact]
    public void AssignCategory_ManualCategorisation_SetsManuallyCategorisedFlag()
    {
        // Arrange
        var transaction = TestHelpers.CreateTransaction();
        var categoryId = Guid.NewGuid();

        // Act
        transaction.AssignCategory(categoryId, isManual: true);

        // Assert
        transaction.UserCategoryId.Should().Be(categoryId);
        transaction.IsManuallyCategorised.Should().BeTrue();
    }

    [Fact]
    public void AssignCategory_AutoOnManuallyCategroised_DoesNotOverwrite()
    {
        // Arrange — user already manually categorised this transaction
        var transaction = TestHelpers.CreateTransaction();
        var manualCategoryId = Guid.NewGuid();
        var autoCategoryId = Guid.NewGuid();

        transaction.AssignCategory(manualCategoryId, isManual: true);

        // Act — rules engine tries to overwrite with a different category
        transaction.AssignCategory(autoCategoryId, isManual: false);

        // Assert — manual choice is preserved
        transaction.UserCategoryId.Should().Be(manualCategoryId);
        transaction.IsManuallyCategorised.Should().BeTrue();
    }

    [Fact]
    public void AssignCategory_ManualOnManuallyCategroised_AllowsOverwrite()
    {
        // Arrange — user already manually categorised this transaction
        var transaction = TestHelpers.CreateTransaction();
        var firstCategoryId = Guid.NewGuid();
        var secondCategoryId = Guid.NewGuid();

        transaction.AssignCategory(firstCategoryId, isManual: true);

        // Act — user changes their mind and picks a different category
        transaction.AssignCategory(secondCategoryId, isManual: true);

        // Assert — new manual choice wins
        transaction.UserCategoryId.Should().Be(secondCategoryId);
        transaction.IsManuallyCategorised.Should().BeTrue();
    }

    [Fact]
    public void UpdateFromProviderSync_SettledTransaction_ThrowsDomainException()
    {
        // Arrange — settled transactions are immutable
        var transaction = TestHelpers.CreateTransaction(
            status: TransactionStatus.Settled);

        // Act
        var act = () => transaction.UpdateFromProviderSync(
            TransactionStatus.Settled, -15m, null);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*already settled*");
    }

    [Fact]
    public void UpdateFromProviderSync_PendingTransaction_UpdatesCorrectly()
    {
        // Arrange
        var transaction = TestHelpers.CreateTransaction(
            status: TransactionStatus.Pending,
            amount: -10m);

        // Act — pending transaction settles with a slightly different amount
        transaction.UpdateFromProviderSync(TransactionStatus.Settled, -12m, 988m);

        // Assert
        transaction.Status.Should().Be(TransactionStatus.Settled);
        transaction.Amount.Should().Be(-12m);
        transaction.RunningBalance.Should().Be(988m);
    }

    [Fact]
    public void Constructor_EmptyExternalTxId_ThrowsDomainException()
    {
        // Arrange & Act
        var act = () => new Transaction(
            accountId: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            externalTxId: "", // invalid
            status: TransactionStatus.Settled,
            transactionType: TransactionType.Debit,
            transactionCategory: "PURCHASE",
            transactionClassification: Array.Empty<string>(),
            description: "Test",
            amount: -10m,
            currency: "GBP",
            transactionDate: DateTimeOffset.UtcNow,
            rawPayload: "{}");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*external transaction id*");
    }

    [Fact]
    public void Constructor_EmptyAccountId_ThrowsDomainException()
    {
        // Arrange & Act
        var act = () => new Transaction(
            accountId: Guid.Empty, // invalid
            userId: Guid.NewGuid(),
            externalTxId: "ext-123",
            status: TransactionStatus.Settled,
            transactionType: TransactionType.Debit,
            transactionCategory: "PURCHASE",
            transactionClassification: Array.Empty<string>(),
            description: "Test",
            amount: -10m,
            currency: "GBP",
            transactionDate: DateTimeOffset.UtcNow,
            rawPayload: "{}");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*valid account*");
    }
}
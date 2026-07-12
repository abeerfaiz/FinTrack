using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using FinTrack.Infrastructure.Persistence;
using FinTrack.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Tests.Repositories;

/// <summary>
/// Integration tests for TransactionRepository against real PostgreSQL.
/// Each test creates its own data and cleans up after itself.
/// Docker must be running with fintrack-db container up.
/// </summary>
public class TransactionRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public TransactionRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetByExternalTxIdAsync_ExistingTransaction_ReturnsTransaction()
    {
        await using var context = _fixture.CreateContext();
        var repo = new TransactionRepository(context);

        // Arrange — create supporting data
        var (user, bankConnection, account) = await CreateTestDataAsync(context);

        var externalTxId = $"test-tx-{Guid.NewGuid()}";
        var transaction = CreateTransaction(account.Id, user.Id, externalTxId);
        await context.Transactions.AddAsync(transaction);
        await context.SaveChangesAsync();

        try
        {
            // Act
            var result = await repo.GetByExternalTxIdAsync(externalTxId);

            // Assert
            result.Should().NotBeNull();
            result!.ExternalTxId.Should().Be(externalTxId);
            result.AccountId.Should().Be(account.Id);
        }
        finally
        {
            // Cleanup — always runs even if test fails
            await CleanupAsync(context, user.Id);
        }
    }

    [Fact]
    public async Task GetByExternalTxIdAsync_NonExistentTransaction_ReturnsNull()
    {
        await using var context = _fixture.CreateContext();
        var repo = new TransactionRepository(context);

        // Act
        var result = await repo.GetByExternalTxIdAsync($"non-existent-{Guid.NewGuid()}");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ValidTransaction_PersistsToDatabase()
    {
        await using var context = _fixture.CreateContext();
        var repo = new TransactionRepository(context);

        var (user, bankConnection, account) = await CreateTestDataAsync(context);
        var externalTxId = $"test-tx-{Guid.NewGuid()}";
        var transaction = CreateTransaction(account.Id, user.Id, externalTxId);

        try
        {
            // Act
            await repo.AddAsync(transaction);
            await context.SaveChangesAsync();

            // Assert — query directly from DB to confirm persisted
            var saved = await context.Transactions
                .FirstOrDefaultAsync(t => t.ExternalTxId == externalTxId);

            saved.Should().NotBeNull();
            saved!.Amount.Should().Be(-25.00m);
            saved.Status.Should().Be(TransactionStatus.Settled);
            saved.UserId.Should().Be(user.Id);
        }
        finally
        {
            await CleanupAsync(context, user.Id);
        }
    }

    [Fact]
    public async Task GetByUserIdAsync_FiltersSettledOnly()
    {
        await using var context = _fixture.CreateContext();
        var repo = new TransactionRepository(context);

        var (user, bankConnection, account) = await CreateTestDataAsync(context);

        // Create one settled and one pending transaction
        var settledTx = CreateTransaction(
            account.Id, user.Id,
            $"settled-{Guid.NewGuid()}",
            TransactionStatus.Settled);

        var pendingTx = CreateTransaction(
            account.Id, user.Id,
            $"pending-{Guid.NewGuid()}",
            TransactionStatus.Pending);

        await context.Transactions.AddRangeAsync(settledTx, pendingTx);
        await context.SaveChangesAsync();

        try
        {
            // Act
            var result = await repo.GetByUserIdAsync(user.Id);

            // Assert — only settled returned, pending excluded
            result.Should().Contain(t => t.ExternalTxId == settledTx.ExternalTxId);
            result.Should().NotContain(t => t.ExternalTxId == pendingTx.ExternalTxId);
        }
        finally
        {
            await CleanupAsync(context, user.Id);
        }
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsCorrectPage()
    {
        await using var context = _fixture.CreateContext();
        var repo = new TransactionRepository(context);

        var (user, bankConnection, account) = await CreateTestDataAsync(context);

        // Create 5 transactions
        var transactions = Enumerable.Range(1, 5).Select(i =>
            CreateTransaction(
                account.Id, user.Id,
                $"paged-tx-{i}-{Guid.NewGuid()}")).ToList();

        await context.Transactions.AddRangeAsync(transactions);
        await context.SaveChangesAsync();

        try
        {
            // Act — page 1 with page size 2
            var (items, totalCount) = await repo.GetPagedAsync(
                userId: user.Id,
                page: 1,
                pageSize: 2);

            // Assert
            totalCount.Should().BeGreaterThanOrEqualTo(5);
            items.Should().HaveCount(2);
        }
        finally
        {
            await CleanupAsync(context, user.Id);
        }
    }

    // ── Helper methods ────────────────────────────────────────────────

    private static async Task<(User, BankConnection, Account)> CreateTestDataAsync(
        FinTrackDbContext context)
    {
        var user = new User(
            $"integration-test-{Guid.NewGuid()}@test.com",
            BCrypt.Net.BCrypt.HashPassword("TestPassword123!"),
            "Integration Test User");

        var bankConnection = new BankConnection(
            userId: user.Id,
            providerId: "truelayer",
            accessTokenEncrypted: "encrypted-test-token",
            refreshTokenEncrypted: "encrypted-test-refresh",
            tokenExpiresAt: DateTimeOffset.UtcNow.AddHours(1));

        var account = new Account(
            bankConnectionId: bankConnection.Id,
            userId: user.Id,
            externalAccountId: $"ext-account-{Guid.NewGuid()}",
            providerId: "truelayer",
            accountType: AccountType.Transaction,
            displayName: "Test Account",
            currency: "GBP",
            tlUpdateTimestamp: DateTimeOffset.UtcNow);

        await context.Users.AddAsync(user);
        await context.BankConnections.AddAsync(bankConnection);
        await context.Accounts.AddAsync(account);
        await context.SaveChangesAsync();

        return (user, bankConnection, account);
    }

    private static Transaction CreateTransaction(
        Guid accountId,
        Guid userId,
        string externalTxId,
        TransactionStatus status = TransactionStatus.Settled)
    {
        return new Transaction(
            accountId: accountId,
            userId: userId,
            externalTxId: externalTxId,
            status: status,
            transactionType: TransactionType.Debit,
            transactionCategory: "PURCHASE",
            transactionClassification: new[] { "Shopping" },
            description: "TEST TRANSACTION",
            amount: -25.00m,
            currency: "GBP",
            transactionDate: DateTimeOffset.UtcNow,
            rawPayload: "{}");
    }

    private static async Task CleanupAsync(FinTrackDbContext context, Guid userId)
    {
        // Delete in FK order: transactions first, then accounts,
        // then bank connections, then users
        var transactions = context.Transactions.Where(t => t.UserId == userId);
        context.Transactions.RemoveRange(transactions);

        var accounts = context.Accounts.Where(a => a.UserId == userId);
        context.Accounts.RemoveRange(accounts);

        var connections = context.BankConnections.Where(bc => bc.UserId == userId);
        context.BankConnections.RemoveRange(connections);

        var users = context.Users.Where(u => u.Id == userId);
        context.Users.RemoveRange(users);

        await context.SaveChangesAsync();
    }
}
using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;

namespace FinTrack.Application.Tests;

/// <summary>
/// Factory methods for creating valid domain entities in tests.
/// Using constructors directly ensures test entities satisfy
/// the same invariants as production code.
/// </summary>
public static class TestHelpers
{
    public static readonly Guid TestUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid OtherUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");

    public static BankConnection CreateBankConnection(Guid? userId = null)
    {
        return new BankConnection(
            userId: userId ?? TestUserId,
            providerId: "truelayer",
            accessTokenEncrypted: "encrypted-access-token",
            refreshTokenEncrypted: "encrypted-refresh-token",
            tokenExpiresAt: DateTimeOffset.UtcNow.AddHours(1));
    }

    public static Account CreateAccount(Guid? bankConnectionId = null, Guid? userId = null)
    {
        return new Account(
            bankConnectionId: bankConnectionId ?? Guid.NewGuid(),
            userId: userId ?? TestUserId,
            externalAccountId: "ext-account-123",
            providerId: "truelayer",
            accountType: AccountType.Transaction,
            displayName: "Test Account",
            currency: "GBP",
            tlUpdateTimestamp: DateTimeOffset.UtcNow);
    }

    public static Transaction CreateTransaction(
        Guid? accountId = null,
        Guid? userId = null,
        string? externalTxId = null,
        TransactionStatus status = TransactionStatus.Settled,
        decimal amount = -10.00m)
    {
        return new Transaction(
            accountId: accountId ?? Guid.NewGuid(),
            userId: userId ?? TestUserId,
            externalTxId: externalTxId ?? Guid.NewGuid().ToString(),
            status: status,
            transactionType: TransactionType.Debit,
            transactionCategory: "PURCHASE",
            transactionClassification: new[] { "Shopping" },
            description: "TEST TRANSACTION",
            amount: amount,
            currency: "GBP",
            transactionDate: DateTimeOffset.UtcNow,
            rawPayload: "{}");
    }

    public static Category CreateSystemCategory(string name = "Groceries")
    {
        return Category.CreateSystemCategory(name, "#22C55E", "shopping-cart");
    }

    public static Category CreateUserCategory(Guid? userId = null, string name = "My Category")
    {
        return Category.CreateUserCategory(
            userId: userId ?? TestUserId,
            name: name,
            colourHex: "#8B5CF6",
            icon: "star");
    }

    public static Budget CreateBudget(
        Guid? userId = null,
        Guid? categoryId = null,
        decimal amount = 300m,
        DateOnly? monthStart = null)
    {
        return new Budget(
            userId: userId ?? TestUserId,
            categoryId: categoryId ?? Guid.NewGuid(),
            amount: amount,
            monthStart: monthStart ?? new DateOnly(2026, 7, 1));
    }
}
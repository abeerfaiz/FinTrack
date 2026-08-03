using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Transactions.Commands.SyncTransactions;

/// <summary>
/// Triggers a full sync for one bank connection — fetches accounts,
/// balances, and transactions from TrueLayer and persists them.
/// Called by the Hangfire recurring job every 6 hours, and by the
/// manual sync endpoint on demand.
/// </summary>
/// <param name="RequestingUserId">
/// The authenticated user asking for this sync, or null when the
/// trusted internal Hangfire job is syncing every active connection.
/// When set, the handler verifies the connection belongs to this user.
/// </param>
public record SyncTransactionsCommand(
    Guid BankConnectionId,
    Guid? RequestingUserId = null) : IRequest<Result<SyncTransactionsResult>>;

public record SyncTransactionsResult(
    int AccountsSynced,
    int TransactionsInserted,
    int TransactionsUpdated);
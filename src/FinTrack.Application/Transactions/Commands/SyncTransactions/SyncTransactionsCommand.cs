using FinTrack.Application.Common.Models;
using MediatR;

namespace FinTrack.Application.Transactions.Commands.SyncTransactions;

/// <summary>
/// Triggers a full sync for one bank connection — fetches accounts,
/// balances, and transactions from TrueLayer and persists them.
/// Called by the Hangfire recurring job every 6 hours, and by the
/// manual sync endpoint on demand.
/// </summary>
public record SyncTransactionsCommand(Guid BankConnectionId) : IRequest<Result<SyncTransactionsResult>>;

public record SyncTransactionsResult(
    int AccountsSynced,
    int TransactionsInserted,
    int TransactionsUpdated);
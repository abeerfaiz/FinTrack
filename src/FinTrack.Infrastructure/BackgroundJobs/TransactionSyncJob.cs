using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Application.Transactions.Commands.SyncTransactions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinTrack.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire recurring job that syncs transactions for every active
/// bank connection every 6 hours. Called by the Hangfire scheduler —
/// never directly from application code.
///
/// Each job execution creates its own DI scope, so DbContext and
/// repositories are fresh per execution — no shared state between runs.
/// This is the correct pattern for Hangfire jobs using scoped services.
/// </summary>
public class TransactionSyncJob
{
    private readonly IBankConnectionRepository _bankConnectionRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<TransactionSyncJob> _logger;

    public TransactionSyncJob(
        IBankConnectionRepository bankConnectionRepository,
        IMediator mediator,
        ILogger<TransactionSyncJob> logger)
    {
        _bankConnectionRepository = bankConnectionRepository;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Syncs all active bank connections. Each connection is processed
    /// independently — a failure on one connection does not abort
    /// the others. Every user's data syncs regardless of whether
    /// another user's bank is temporarily unavailable.
    /// </summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Transaction sync job started");

        // Fetch every active connection across all users.
        // GetExpiringSoonAsync would only return connections needing
        // token refresh — we want ALL active connections for sync.
        var connections = await _bankConnectionRepository
            .GetActiveConnectionsAsync(cancellationToken);

        if (!connections.Any())
        {
            _logger.LogInformation("No active bank connections found — sync job complete");
            return;
        }

        _logger.LogInformation(
            "Syncing {Count} active bank connections",
            connections.Count);

        var successCount = 0;
        var failureCount = 0;

        foreach (var connection in connections)
        {
            try
            {
                var result = await _mediator.Send(
                    new SyncTransactionsCommand(connection.Id),
                    cancellationToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation(
                        "Synced connection {ConnectionId}: {Accounts} accounts, " +
                        "{Inserted} inserted, {Updated} updated",
                        connection.Id,
                        result.Value!.AccountsSynced,
                        result.Value.TransactionsInserted,
                        result.Value.TransactionsUpdated);

                    successCount++;
                }
                else
                {
                    _logger.LogWarning(
                        "Sync failed for connection {ConnectionId}: {Error}",
                        connection.Id,
                        result.Error);

                    failureCount++;
                }
            }
            catch (Exception ex)
            {
                // Log and continue — never let one connection's failure
                // prevent other users' data from syncing
                _logger.LogError(ex,
                    "Unhandled error syncing connection {ConnectionId}",
                    connection.Id);

                failureCount++;
            }
        }

        _logger.LogInformation(
            "Transaction sync job complete — {Success} succeeded, {Failure} failed",
            successCount,
            failureCount);
    }
}
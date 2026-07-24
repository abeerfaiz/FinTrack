using Hangfire;
using Microsoft.Extensions.Logging;

namespace FinTrack.Infrastructure.BackgroundJobs;

/// <summary>
/// Registers all Hangfire recurring jobs on application startup.
/// Called once from Program.cs after the app is built.
/// Uses IRecurringJobManager (service-based API) instead of the
/// static RecurringJob class — the static API requires JobStorage.Current
/// to be set which is not guaranteed on Azure App Service startup.
/// </summary>
public static class JobScheduler
{
    public static void RegisterRecurringJobs(
        IRecurringJobManager recurringJobManager,
        ILogger logger)
    {
        // Transaction sync — every 6 hours
        recurringJobManager.AddOrUpdate<TransactionSyncJob>(
            recurringJobId: "transaction-sync-all-connections",
            methodCall: job => job.ExecuteAsync(CancellationToken.None),
            cronExpression: "0 */6 * * *",
            options: new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });

        // Token refresh — every 4 minutes
        recurringJobManager.AddOrUpdate<TokenRefreshJob>(
            recurringJobId: "token-refresh-expiring-soon",
            methodCall: job => job.ExecuteAsync(CancellationToken.None),
            cronExpression: "*/4 * * * *",
            options: new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });

        logger.LogInformation(
            "Hangfire recurring jobs registered: transaction-sync (6h), token-refresh (4m)");
    }
}
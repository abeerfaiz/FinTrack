using Hangfire;
using Microsoft.Extensions.Logging;

namespace FinTrack.Infrastructure.BackgroundJobs;

/// <summary>
/// Registers all Hangfire recurring jobs on application startup.
/// Called once from Program.cs after the app is built.
///
/// Uses CRON expressions for scheduling:
/// - "0 */6 * * *" = every 6 hours at minute 0
/// - "*/4 * * * *" = every 4 minutes
///
/// Job IDs are stable string constants — if a job with the same ID
/// already exists in Hangfire's PostgreSQL storage, it is updated
/// rather than duplicated. Safe to call on every restart.
/// </summary>
public static class JobScheduler
{
	public static void RegisterRecurringJobs(ILogger logger)
	{
		// Transaction sync — every 6 hours
		// Syncs all active bank connections across all users
		RecurringJob.AddOrUpdate<TransactionSyncJob>(
			recurringJobId: "transaction-sync-all-connections",
			methodCall: job => job.ExecuteAsync(CancellationToken.None),
			cronExpression: "0 */6 * * *",
			options: new RecurringJobOptions
			{
				TimeZone = TimeZoneInfo.Utc
			});

		// Token refresh — every 4 minutes
		// Proactively refreshes any token expiring within 5 minutes
		RecurringJob.AddOrUpdate<TokenRefreshJob>(
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
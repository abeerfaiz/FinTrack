using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using System.Net;

namespace FinTrack.Infrastructure.OpenBanking;

/// <summary>
/// Defines the resilience pipeline for every call to TrueLayer's Data API.
/// Three patterns compose together: retry transient failures, break the
/// circuit if failures persist, and never crash the whole sync job over
/// one bank's temporary unavailability.
/// </summary>
public static class TrueLayerResiliencePolicies
{
	public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILogger logger)
	{
		// Retries on: HTTP 5xx, HTTP 408 (timeout), and network failures
		// (HttpRequestException). Never retries 4xx errors other than 408 —
		// a 401 or 400 will fail identically every time, so retrying
		// wastes time and obscures the real error.
		return HttpPolicyExtensions
			.HandleTransientHttpError()
			.OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
			.WaitAndRetryAsync(
				retryCount: 3,
				sleepDurationProvider: attempt =>
					TimeSpan.FromSeconds(Math.Pow(2, attempt)), // 2s, 4s, 8s
				onRetry: (outcome, timespan, attempt, context) =>
				{
					logger.LogWarning(
						"TrueLayer request failed, retrying in {Delay}s (attempt {Attempt}/3). Status: {StatusCode}",
						timespan.TotalSeconds,
						attempt,
						outcome.Result?.StatusCode);
				});
	}

	public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ILogger logger)
	{
		// After 5 consecutive failures, the circuit opens for 30 seconds.
		// All calls during that window fail immediately without hitting
		// TrueLayer at all — protects against hammering a degraded service
		// and exhausting the connection pool.
		return HttpPolicyExtensions
			.HandleTransientHttpError()
			.CircuitBreakerAsync(
				handledEventsAllowedBeforeBreaking: 5,
				durationOfBreak: TimeSpan.FromSeconds(30),
				onBreak: (outcome, breakDuration) =>
				{
					logger.LogError(
						"TrueLayer circuit breaker opened for {Duration}s after repeated failures. Status: {StatusCode}",
						breakDuration.TotalSeconds,
						outcome.Result?.StatusCode);
				},
				onReset: () =>
				{
					logger.LogInformation("TrueLayer circuit breaker reset — calls resuming normally.");
				},
				onHalfOpen: () =>
				{
					logger.LogInformation("TrueLayer circuit breaker half-open — testing one request.");
				});
	}
}
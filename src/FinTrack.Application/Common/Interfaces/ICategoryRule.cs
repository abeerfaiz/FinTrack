namespace FinTrack.Application.Common.Interfaces;

/// <summary>
/// Finds the highest-priority category rule matching a given merchant
/// name for a specific user. Returns null if no rule matches.
/// Matching is case-insensitive — keywords are stored uppercase,
/// merchant names are uppercased before comparison.
/// </summary>
public interface ICategoryRulesEngine
{
    Task<Guid?> FindMatchAsync(
        Guid userId,
        string? merchantName,
        string description,
        CancellationToken cancellationToken = default);
}
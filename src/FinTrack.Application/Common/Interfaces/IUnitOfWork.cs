namespace FinTrack.Application.Common.Interfaces;

/// <summary>
/// Represents a single transactional boundary — commit everything
/// changed across however many repositories were touched in this
/// handler, or commit nothing. Implemented in Infrastructure by
/// wrapping FinTrackDbContext.SaveChangesAsync().
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
using FinTrack.Application.Common.Interfaces;

namespace FinTrack.Infrastructure.Persistence;

/// <summary>
/// Wraps FinTrackDbContext.SaveChangesAsync() behind the IUnitOfWork
/// interface. This means Application handlers can commit all pending
/// changes from any number of repositories in one atomic operation,
/// without having a direct dependency on EF Core or FinTrackDbContext.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly FinTrackDbContext _context;

    public UnitOfWork(FinTrackDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
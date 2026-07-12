using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Persistence.Repositories;

public class DirectDebitRepository : IDirectDebitRepository
{
    private readonly FinTrackDbContext _context;

    public DirectDebitRepository(FinTrackDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<DirectDebit>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.DirectDebits
            .Where(d => d.UserId == userId)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }
}
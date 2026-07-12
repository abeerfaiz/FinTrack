using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Persistence.Repositories;

public class StandingOrderRepository : IStandingOrderRepository
{
    private readonly FinTrackDbContext _context;

    public StandingOrderRepository(FinTrackDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StandingOrder>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.StandingOrders
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.NextPaymentDate)
            .ToListAsync(cancellationToken);
    }
}
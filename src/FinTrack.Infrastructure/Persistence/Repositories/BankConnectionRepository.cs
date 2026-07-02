using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Persistence.Repositories;

public class BankConnectionRepository : IBankConnectionRepository
{
    private readonly FinTrackDbContext _context;

    public BankConnectionRepository(FinTrackDbContext context)
    {
        _context = context;
    }

    public async Task<BankConnection?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.BankConnections
            .FirstOrDefaultAsync(bc => bc.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<BankConnection>> GetActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.BankConnections
            .Where(bc => bc.UserId == userId
                      && bc.Status == BankConnectionStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BankConnection>> GetExpiringSoonAsync(
        CancellationToken cancellationToken = default)
    {
        // Find every active connection whose token expires within
        // the next 5 minutes. The background token refresh job calls
        // this to proactively refresh before expiry, not reactively
        // after a failed API call — the correct pattern for token
        // lifecycle management in any OAuth2 integration.
        var threshold = DateTimeOffset.UtcNow.AddMinutes(5);

        return await _context.BankConnections
            .Where(bc => bc.Status == BankConnectionStatus.Active
                      && bc.TokenExpiresAt <= threshold)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BankConnection>> GetActiveConnectionsAsync(
    CancellationToken cancellationToken = default)
    {
        return await _context.BankConnections
            .Where(bc => bc.Status == BankConnectionStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        BankConnection connection,
        CancellationToken cancellationToken = default)
    {
        await _context.BankConnections.AddAsync(connection, cancellationToken);
    }

    public void Update(BankConnection connection)
    {
        _context.BankConnections.Update(connection);
    }
}
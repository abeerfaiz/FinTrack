using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Persistence.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly FinTrackDbContext _context;

    public AccountRepository(FinTrackDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Account?> GetByExternalAccountIdAsync(
        string externalAccountId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Scoped to userId — TrueLayer's sandbox mock provider returns the
        // same fixed external account ids for every connection, so matching
        // on externalAccountId alone would attach one user's mock accounts
        // to whichever other user connects next.
        return await _context.Accounts
            .FirstOrDefaultAsync(a =>
                a.ExternalAccountId == externalAccountId && a.UserId == userId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Account>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Where(a => a.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Account>> GetByBankConnectionIdAsync(
        Guid bankConnectionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Where(a => a.BankConnectionId == bankConnectionId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Account account,
        CancellationToken cancellationToken = default)
    {
        await _context.Accounts.AddAsync(account, cancellationToken);
    }

    public void Update(Account account)
    {
        // Only attach + force Modified for entities coming from outside
        // this DbContext's change tracker. If the account is already
        // tracked (e.g. still in the Added state from an earlier
        // AddAsync in the same unit of work), EF Core's change tracker
        // already picks up property mutations on that same instance —
        // calling Update() here would incorrectly flip Added to
        // Modified, turning the pending INSERT into an UPDATE for a
        // row that doesn't exist yet.
        var entry = _context.Entry(account);
        if (entry.State == EntityState.Detached)
        {
            _context.Accounts.Update(account);
        }
    }
}
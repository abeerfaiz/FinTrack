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
        CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.ExternalAccountId == externalAccountId, cancellationToken);
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
        _context.Accounts.Update(account);
    }
}
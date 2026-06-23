using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Domain.Entities;
using FinTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Persistence.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly FinTrackDbContext _context;

    public TransactionRepository(FinTrackDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Transaction?> GetByExternalTxIdAsync(
        string externalTxId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .FirstOrDefaultAsync(t => t.ExternalTxId == externalTxId, cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> GetByUserIdAsync(
        Guid userId,
        DateOnly? from = null,
        DateOnly? to = null,
        Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions
            .Where(t => t.UserId == userId
                     && t.Status == TransactionStatus.Settled
                     && !t.IsArchived);

        if (from.HasValue)
        {
            var fromDateTime = from.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(t => t.TransactionDate >= fromDateTime);
        }

        if (to.HasValue)
        {
            var toDateTime = to.Value.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
            query = query.Where(t => t.TransactionDate <= toDateTime);
        }

        if (categoryId.HasValue)
            query = query.Where(t => t.UserCategoryId == categoryId.Value);

        return await query
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> GetUncategorisedAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Where(t => t.UserId == userId
                     && t.Status == TransactionStatus.Settled
                     && !t.IsManuallyCategorised
                     && t.UserCategoryId == null)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Transaction transaction,
        CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
    }

    public void Update(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
    }
}
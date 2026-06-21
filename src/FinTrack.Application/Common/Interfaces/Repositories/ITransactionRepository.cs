using FinTrack.Domain.Entities;

namespace FinTrack.Application.Common.Interfaces.Repositories;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Used by the sync job to check idempotency before constructing
    /// a new Transaction — if this returns non-null, skip the insert.
    /// </summary>
    Task<Transaction?> GetByExternalTxIdAsync(string externalTxId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Transaction>> GetByUserIdAsync(
        Guid userId,
        DateOnly? from = null,
        DateOnly? to = null,
        Guid? categoryId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Transactions eligible for the auto-categorisation rules engine —
    /// settled, not yet manually categorised by the user.
    /// </summary>
    Task<IReadOnlyList<Transaction>> GetUncategorisedAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// EF Core change tracking means this is often a no-op call —
    /// the entity is already tracked and modified in-memory.
    /// Kept explicit for readability and to match a consistent
    /// repository interface shape across all entities.
    /// </summary>
    void Update(Transaction transaction);
}
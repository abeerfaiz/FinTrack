using FinTrack.Domain.Entities;

namespace FinTrack.Application.Common.Interfaces.Repositories;

public interface IBankConnectionRepository
{
    Task<BankConnection?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BankConnection>> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Used by the token refresh background job — finds every active
    /// connection whose token is expiring soon, across all users.
    /// </summary>
    Task<IReadOnlyList<BankConnection>> GetExpiringSoonAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns every active bank connection across all users.
    /// Used by the background sync job which processes all users,
    /// unlike GetActiveByUserIdAsync which scopes to one user.
    /// </summary>
    Task<IReadOnlyList<BankConnection>> GetActiveConnectionsAsync(CancellationToken cancellationToken = default);

    Task AddAsync(BankConnection connection, CancellationToken cancellationToken = default);
    void Update(BankConnection connection);
}
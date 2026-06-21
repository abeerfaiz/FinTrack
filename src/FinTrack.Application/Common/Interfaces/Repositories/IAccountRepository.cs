using FinTrack.Domain.Entities;

namespace FinTrack.Application.Common.Interfaces.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Account?> GetByExternalAccountIdAsync(string externalAccountId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Account>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Account>> GetByBankConnectionIdAsync(Guid bankConnectionId, CancellationToken cancellationToken = default);
    Task AddAsync(Account account, CancellationToken cancellationToken = default);
    void Update(Account account);
}
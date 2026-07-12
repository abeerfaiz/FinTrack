using FinTrack.Domain.Entities;

namespace FinTrack.Application.Common.Interfaces.Repositories;

public interface IDirectDebitRepository
{
    Task<IReadOnlyList<DirectDebit>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
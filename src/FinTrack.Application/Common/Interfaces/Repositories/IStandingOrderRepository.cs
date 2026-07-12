using FinTrack.Domain.Entities;

namespace FinTrack.Application.Common.Interfaces.Repositories;

public interface IStandingOrderRepository
{
    Task<IReadOnlyList<StandingOrder>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
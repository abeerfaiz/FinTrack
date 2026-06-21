using FinTrack.Domain.Entities;

namespace FinTrack.Application.Common.Interfaces.Repositories;

public interface IBudgetRepository
{
    Task<Budget?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Budget?> GetByUserCategoryMonthAsync(
        Guid userId,
        Guid categoryId,
        DateOnly monthStart,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Budget>> GetByUserAndMonthAsync(
        Guid userId,
        DateOnly monthStart,
        CancellationToken cancellationToken = default);

    Task AddAsync(Budget budget, CancellationToken cancellationToken = default);
    void Update(Budget budget);
}
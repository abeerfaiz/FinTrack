using FinTrack.Domain.Entities;

namespace FinTrack.Application.Common.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns system categories (user_id null) plus this user's own
    /// custom categories — the exact "OR" query we designed in Week 1.
    /// </summary>
    Task<IReadOnlyList<Category>> GetAvailableForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(Category category, CancellationToken cancellationToken = default);
    void Update(Category category);
}
using FinTrack.Domain.Entities;

namespace FinTrack.Application.Common.Interfaces.Repositories;

public interface ICategoryRuleRepository
{
    Task<IReadOnlyList<CategoryRule>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(CategoryRule rule, CancellationToken cancellationToken = default);

    void Update(CategoryRule rule);

    void Delete(CategoryRule rule);
}
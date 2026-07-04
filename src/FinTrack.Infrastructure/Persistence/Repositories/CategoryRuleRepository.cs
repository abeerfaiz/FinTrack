using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Persistence.Repositories;

public class CategoryRuleRepository : ICategoryRuleRepository
{
    private readonly FinTrackDbContext _context;

    public CategoryRuleRepository(FinTrackDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CategoryRule>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Ordered by priority ascending — lowest number wins.
        // The rules engine takes the first match from this list.
        return await _context.CategoryRules
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        CategoryRule rule,
        CancellationToken cancellationToken = default)
    {
        await _context.CategoryRules.AddAsync(rule, cancellationToken);
    }

    public void Update(CategoryRule rule)
    {
        var entry = _context.Entry(rule);
        if (entry.State == EntityState.Detached)
            _context.CategoryRules.Update(rule);
    }

    public void Delete(CategoryRule rule)
    {
        _context.CategoryRules.Remove(rule);
    }
}
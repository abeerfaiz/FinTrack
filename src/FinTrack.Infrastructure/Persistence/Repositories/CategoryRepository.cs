using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Persistence.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly FinTrackDbContext _context;

    public CategoryRepository(FinTrackDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetAvailableForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // System categories (user_id null) shared by everyone,
        // plus this specific user's own custom categories.
        // The HasQueryFilter on CategoryConfiguration already excludes
        // soft-deleted rows — we never need to write deleted_at IS NULL
        // anywhere in repository queries.
        return await _context.Categories
            .Where(c => c.UserId == null || c.UserId == userId)
            .OrderBy(c => c.IsSystem)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Category category,
        CancellationToken cancellationToken = default)
    {
        await _context.Categories.AddAsync(category, cancellationToken);
    }

    public void Update(Category category)
    {
        _context.Categories.Update(category);
    }
}
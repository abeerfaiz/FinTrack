using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Persistence.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly FinTrackDbContext _context;

    public BudgetRepository(FinTrackDbContext context)
    {
        _context = context;
    }

    public async Task<Budget?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Budgets
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<Budget?> GetByUserCategoryMonthAsync(
        Guid userId,
        Guid categoryId,
        DateOnly monthStart,
        CancellationToken cancellationToken = default)
    {
        // Normalise to first of month here too as a safety net —
        // even if the caller forgot, the query finds the right row.
        var normalisedMonth = new DateOnly(monthStart.Year, monthStart.Month, 1);

        return await _context.Budgets
            .FirstOrDefaultAsync(
                b => b.UserId == userId
                  && b.CategoryId == categoryId
                  && b.MonthStart == normalisedMonth,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Budget>> GetByUserAndMonthAsync(
        Guid userId,
        DateOnly monthStart,
        CancellationToken cancellationToken = default)
    {
        var normalisedMonth = new DateOnly(monthStart.Year, monthStart.Month, 1);

        return await _context.Budgets
            .Where(b => b.UserId == userId && b.MonthStart == normalisedMonth)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        Budget budget,
        CancellationToken cancellationToken = default)
    {
        await _context.Budgets.AddAsync(budget, cancellationToken);
    }

    public void Update(Budget budget)
    {
        _context.Budgets.Update(budget);
    }
}
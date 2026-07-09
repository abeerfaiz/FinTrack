using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinTrack.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly FinTrackDbContext _context;

    public UserRepository(FinTrackDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(
                u => u.Email == email.Trim().ToLowerInvariant(),
                cancellationToken);
    }

    public async Task AddAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public void Update(User user)
    {
        var entry = _context.Entry(user);
        if (entry.State == EntityState.Detached)
            _context.Users.Update(user);
    }

    public async Task<User?> GetByRefreshTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(
                u => u.RefreshToken == tokenHash,
                cancellationToken);
    }
}
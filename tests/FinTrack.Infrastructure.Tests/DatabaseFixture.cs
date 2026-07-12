using FinTrack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FinTrack.Infrastructure.Tests;

/// <summary>
/// Provides a real FinTrackDbContext connected to the local Docker
/// PostgreSQL instance for integration tests. Each test class that
/// uses this fixture gets a fresh DbContext per test via CreateContext().
/// Tests are responsible for cleaning up their own data.
/// </summary>
public class DatabaseFixture : IDisposable
{
    private readonly string _connectionString;

    public DatabaseFixture()
    {
        // Use the same connection string as the main app
        // Docker PostgreSQL must be running for integration tests to pass
        _connectionString =
            "Host=localhost;Port=5432;Database=fintrack;" +
            "Username=fintrack_user;Password=fintrack_pass";
    }

    public FinTrackDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FinTrackDbContext>()
            .UseNpgsql(_connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new FinTrackDbContext(options);
    }

    public void Dispose() { }
}
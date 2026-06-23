using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Infrastructure.Persistence;
using FinTrack.Infrastructure.Persistence.Repositories;
using FinTrack.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinTrack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core DbContext — Scoped lifetime means one DbContext
        // instance per HTTP request, shared across all repositories
        // and the UnitOfWork within that request. This is what makes
        // SaveChangesAsync() commit everything from all repositories
        // together in one transaction — they all share the same
        // DbContext instance, and therefore the same change tracker.
        services.AddDbContext<FinTrackDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("FinTrackDb"))
                   .UseSnakeCaseNamingConvention());

        // Repositories — Scoped to match DbContext lifetime.
        // Never Singleton — a singleton repository sharing one
        // DbContext instance across all requests is a threading
        // disaster waiting to happen.
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<IBankConnectionRepository, BankConnectionRepository>();

        // Unit of Work — same Scoped lifetime, same reasoning.
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Token encryption — Singleton is safe here because
        // AesTokenEncryptionService holds only immutable state
        // (the encryption key byte array) with no per-request context.
        // Singleton means one instance created once and reused forever,
        // which is appropriate for stateless services.
        services.AddSingleton<ITokenEncryptionService, AesTokenEncryptionService>();

        return services;
    }
}
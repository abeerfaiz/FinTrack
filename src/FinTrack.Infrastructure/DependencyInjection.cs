using FinTrack.Application.Common.Interfaces;
using FinTrack.Application.Common.Interfaces.Repositories;
using FinTrack.Infrastructure.OpenBanking;
using FinTrack.Infrastructure.Persistence;
using FinTrack.Infrastructure.Persistence.Repositories;
using FinTrack.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

        // Bind TrueLayer configuration section to strongly-typed options.
        // Validates that required values are present at startup rather than
        // failing silently at the first API call.
        services.AddOptions<TrueLayerOptions>()
            .Bind(configuration.GetSection(TrueLayerOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // TrueLayer auth HttpClient — pointed at the auth base URL
        services.AddHttpClient<TrueLayerAuthService>(client =>
        {
            client.BaseAddress = new Uri(
                configuration["TrueLayer:AuthBaseUrl"]
                ?? throw new InvalidOperationException("TrueLayer:AuthBaseUrl missing."));
        });

        // Explicitly register TrueLayerAuthService so it can be injected
        // into TrueLayerClient. Without this, the DI container knows how
        // to create it for direct requests but not as a dependency of
        // another service registered via AddHttpClient.
        services.AddScoped<TrueLayerAuthService>();

        // Named HttpClient for TrueLayer Data API — name matches what
        // TrueLayerClient requests from IHttpClientFactory above.
        services.AddHttpClient("TrueLayerDataApi", client =>
        {
            client.BaseAddress = new Uri(
                configuration["TrueLayer:ApiBaseUrl"]
                ?? throw new InvalidOperationException("TrueLayer:ApiBaseUrl missing."));
        })
        .AddPolicyHandler((serviceProvider, _) =>
            TrueLayerResiliencePolicies.GetRetryPolicy(
                serviceProvider.GetRequiredService<ILogger<TrueLayerClient>>()))
        .AddPolicyHandler((serviceProvider, _) =>
            TrueLayerResiliencePolicies.GetCircuitBreakerPolicy(
                serviceProvider.GetRequiredService<ILogger<TrueLayerClient>>()));

        // Register TrueLayerClient as the IOpenBankingClient implementation.
        // Scoped — matches repository and DbContext lifetimes.
        services.AddScoped<IOpenBankingClient, TrueLayerClient>();

        // Register TrueLayerClient as the implementation of IOpenBankingClient.
        // Scoped because it depends on TrueLayerAuthService which is HTTP-client
        // lifecycle managed.
        services.AddScoped<IOpenBankingClient, TrueLayerClient>();

        return services;
    }
}
using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FinTrack.Infrastructure.Persistence;

public class FinTrackDbContext : DbContext
{
    public FinTrackDbContext(DbContextOptions<FinTrackDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<BankConnection> BankConnections => Set<BankConnection>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategoryRule> CategoryRules => Set<CategoryRule>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<DirectDebit> DirectDebits => Set<DirectDebit>();
    public DbSet<StandingOrder> StandingOrders => Set<StandingOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Rather than configuring every entity's mapping rules inline here
        // (which becomes an unreadable 500-line method as the project grows),
        // we scan this assembly for any class implementing
        // IEntityTypeConfiguration<T> and apply it automatically.
        // This is what the Configurations/ folder is for.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
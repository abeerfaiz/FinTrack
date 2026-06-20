using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("budgets");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(b => b.MonthStart)
            .HasColumnName("month_start")
            .HasColumnType("date") // DateOnly maps to PostgreSQL DATE, not TIMESTAMPTZ
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(b => b.DeletedAt)
            .HasColumnName("deleted_at");

        builder.HasQueryFilter(b => b.DeletedAt == null);

        // Relationship to Category: Restrict.
        // Deleting a category that still has an active budget should
        // fail loudly rather than silently cascading the budget away
        // or leaving it orphaned — this forces the application layer
        // to handle it explicitly (e.g. "remove the budget first").
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // One budget per user, per category, per month — this unique
        // composite index is a real business rule enforced at the
        // database level, not just application logic. Prevents
        // accidentally creating two £300 Groceries budgets for the
        // same month through a race condition or a UI bug.
        builder.HasIndex(b => new { b.UserId, b.CategoryId, b.MonthStart })
            .IsUnique();
    }
}
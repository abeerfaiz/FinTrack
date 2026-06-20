using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public class CategoryRuleConfiguration : IEntityTypeConfiguration<CategoryRule>
{
    public void Configure(EntityTypeBuilder<CategoryRule> builder)
    {
        builder.ToTable("category_rules");

        builder.HasKey(cr => cr.Id);

        builder.Property(cr => cr.Keyword)
            .HasColumnName("keyword")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(cr => cr.Priority)
            .HasColumnName("priority")
            .IsRequired();

        builder.Property(cr => cr.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Relationship to Category: Cascade.
        // A rule that targets a category makes no sense once that
        // category is deleted — there's no orphaned-rule scenario
        // worth preserving.
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(cr => cr.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Single composite index. Covers both "all rules for this user"
        // (uses just the UserId prefix) and "all rules for this user
        // ordered by priority" (uses the full index) — the rules
        // engine's actual query shape. No separate single-column
        // UserId index needed; it would be a redundant duplicate
        // of this index's leading column.
        builder.HasIndex(cr => new { cr.UserId, cr.Priority });
    }
}
using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(c => c.Id);

        // UserId nullable — null means system category (is_system = true),
        // shared across every user. Set means a user-created category.
        builder.Property(c => c.UserId)
            .HasColumnName("user_id");

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.ColourHex)
            .HasColumnName("colour_hex")
            .IsRequired()
            .HasMaxLength(7); // "#RRGGBB"

        builder.Property(c => c.Icon)
            .HasColumnName("icon")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.IsSystem)
            .HasColumnName("is_system")
            .HasDefaultValue(false);

        builder.Property(c => c.DeletedAt)
            .HasColumnName("deleted_at");

        // Soft delete query filter — every query against Categories
        // automatically excludes deleted rows, without needing to
        // remember "WHERE deleted_at IS NULL" in every single query
        // across the entire codebase. This is applied globally here,
        // once, in the configuration.
        builder.HasQueryFilter(c => c.DeletedAt == null);

        // No relationship to User configured with HasOne/WithMany here
        // because UserId is nullable and represents an optional owner,
        // not a required parent-child relationship. EF Core would
        // otherwise try to enforce referential integrity in a way
        // that conflicts with system categories having no user at all.
        builder.HasIndex(c => c.UserId);
    }
}
using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        // Emails must be unique — no two accounts can share a login.
        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired();

        builder.Property(u => u.DisplayName)
            .HasColumnName("display_name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}
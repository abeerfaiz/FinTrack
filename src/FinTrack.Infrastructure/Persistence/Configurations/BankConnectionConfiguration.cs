using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public class BankConnectionConfiguration : IEntityTypeConfiguration<BankConnection>
{
    public void Configure(EntityTypeBuilder<BankConnection> builder)
    {
        builder.ToTable("bank_connections");

        builder.HasKey(bc => bc.Id);

        builder.Property(bc => bc.ProviderId)
            .HasColumnName("provider_id")
            .IsRequired()
            .HasMaxLength(50);

        // Tokens are stored encrypted by ITokenEncryptionService
        // before they ever reach this entity — what's persisted
        // here is ciphertext, never plaintext. No max length cap
        // since AES-256 ciphertext length varies with input.
        builder.Property(bc => bc.AccessTokenEncrypted)
            .HasColumnName("access_token_encrypted")
            .IsRequired();

        builder.Property(bc => bc.RefreshTokenEncrypted)
            .HasColumnName("refresh_token_encrypted")
            .IsRequired();

        builder.Property(bc => bc.TokenExpiresAt)
            .HasColumnName("token_expires_at")
            .IsRequired();

        // The 90-day PSD2 re-consent clock starts here. A background
        // job checks DateTimeOffset.UtcNow > ConsentCreatedAt.AddDays(90)
        // to flag connections needing re-authorisation.
        builder.Property(bc => bc.ConsentCreatedAt)
            .HasColumnName("consent_created_at")
            .IsRequired();

        builder.Property(bc => bc.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(bc => bc.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Relationship to User: Cascade.
        // If a user account is deleted (GDPR erasure request),
        // every bank connection — and everything cascading from it
        // (accounts, transactions) — must be removed too.
        builder.HasOne<User>()
            .WithMany(u => u.BankConnections)
            .HasForeignKey(bc => bc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(bc => bc.UserId);
    }
}
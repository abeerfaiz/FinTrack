using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public class DirectDebitConfiguration : IEntityTypeConfiguration<DirectDebit>
{
    public void Configure(EntityTypeBuilder<DirectDebit> builder)
    {
        builder.ToTable("direct_debits");

        builder.HasKey(dd => dd.Id);

        builder.HasIndex(dd => dd.ExternalDirectDebitId)
            .IsUnique();

        builder.Property(dd => dd.ExternalDirectDebitId)
            .HasColumnName("external_direct_debit_id")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(dd => dd.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(dd => dd.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(dd => dd.PreviousPaymentAmount)
            .HasColumnName("previous_payment_amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(dd => dd.PreviousPaymentDate)
            .HasColumnName("previous_payment_date")
            .IsRequired();

        builder.Property(dd => dd.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(dd => dd.RawPayload)
            .HasColumnName("raw_payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(dd => dd.LastSyncedAt)
            .HasColumnName("last_synced_at")
            .IsRequired();

        // Relationship to Account: Cascade — same reasoning as
        // Transaction. A direct debit record has no meaning once
        // its account is gone.
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(dd => dd.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(dd => dd.UserId);
    }
}
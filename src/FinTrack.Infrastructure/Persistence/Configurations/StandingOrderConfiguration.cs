using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public class StandingOrderConfiguration : IEntityTypeConfiguration<StandingOrder>
{
    public void Configure(EntityTypeBuilder<StandingOrder> builder)
    {
        builder.ToTable("standing_orders");

        builder.HasKey(so => so.Id);

        builder.Property(so => so.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasMaxLength(20);

        // Raw ISO 20022 frequency string, e.g. "IntrvlMnthDay:01:26".
        // Stored as-is — parsing into human-readable text ("monthly
        // on the 26th") happens in the Application layer, not here.
        builder.Property(so => so.Frequency)
            .HasColumnName("frequency")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(so => so.Reference)
            .HasColumnName("reference")
            .HasMaxLength(255);

        builder.Property(so => so.Payee)
            .HasColumnName("payee")
            .HasMaxLength(255);

        builder.Property(so => so.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(so => so.NextPaymentDate)
            .HasColumnName("next_payment_date");

        builder.Property(so => so.NextPaymentAmount)
            .HasColumnName("next_payment_amount")
            .HasColumnType("decimal(18,2)");

        builder.Property(so => so.FirstPaymentDate)
            .HasColumnName("first_payment_date");

        builder.Property(so => so.FirstPaymentAmount)
            .HasColumnName("first_payment_amount")
            .HasColumnType("decimal(18,2)");

        builder.Property(so => so.FinalPaymentDate)
            .HasColumnName("final_payment_date");

        builder.Property(so => so.FinalPaymentAmount)
            .HasColumnName("final_payment_amount")
            .HasColumnType("decimal(18,2)");

        builder.Property(so => so.RawPayload)
            .HasColumnName("raw_payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(so => so.LastSyncedAt)
            .HasColumnName("last_synced_at")
            .IsRequired();

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(so => so.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(so => so.UserId);
    }
}
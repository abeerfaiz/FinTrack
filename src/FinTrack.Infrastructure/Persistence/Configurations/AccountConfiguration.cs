using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(a => a.Id);

        // Idempotency guarantee for account sync, same pattern as
        // Transaction.ExternalTxId — prevents duplicate account rows
        // if the sync job re-processes the same TrueLayer account.
        builder.HasIndex(a => a.ExternalAccountId)
            .IsUnique();

        builder.Property(a => a.ExternalAccountId)
            .HasColumnName("external_account_id")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.ProviderId)
            .HasColumnName("provider_id")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.AccountType)
            .HasColumnName("account_type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(a => a.DisplayName)
            .HasColumnName("display_name")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        // All account number fields nullable — confirmed from the
        // actual TrueLayer responses that swift_bic is inconsistent
        // between the list and single-account endpoints.
        builder.Property(a => a.SortCode)
            .HasColumnName("sort_code")
            .HasMaxLength(20);

        builder.Property(a => a.AccountNumber)
            .HasColumnName("account_number")
            .HasMaxLength(50);

        builder.Property(a => a.Iban)
            .HasColumnName("iban")
            .HasMaxLength(50);

        builder.Property(a => a.SwiftBic)
            .HasColumnName("swift_bic")
            .HasMaxLength(20);

        // Three separate balance figures, all nullable — balance
        // comes from a separate API call from account details,
        // so a freshly synced account may have no balance data yet.
        builder.Property(a => a.BalanceCurrent)
            .HasColumnName("balance_current")
            .HasColumnType("decimal(18,2)");

        builder.Property(a => a.BalanceAvailable)
            .HasColumnName("balance_available")
            .HasColumnType("decimal(18,2)");

        builder.Property(a => a.BalanceOverdraft)
            .HasColumnName("balance_overdraft")
            .HasColumnType("decimal(18,2)");

        builder.Property(a => a.BalanceUpdatedAt)
            .HasColumnName("balance_updated_at");

        builder.Property(a => a.LastSyncedAt)
            .HasColumnName("last_synced_at");

        builder.Property(a => a.TlUpdateTimestamp)
            .HasColumnName("tl_update_timestamp")
            .IsRequired();

        // Relationship to BankConnection: Cascade.
        // Deleting a bank connection (user revokes access) means
        // every account under it should go too — they have no
        // meaning without the connection that authorised reading them.
        builder.HasOne<BankConnection>()
            .WithMany(bc => bc.Accounts)
            .HasForeignKey(a => a.BankConnectionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Direct user_id FK — the denormalisation pattern we agreed
        // on for query performance. "Show me this user's accounts"
        // never needs to join through bank_connections.
        builder.HasIndex(a => a.UserId);
    }
}
using FinTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinTrack.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(t => t.Id);

        // The idempotency guarantee we designed in Week 1.
        // This unique index is what makes ON CONFLICT DO NOTHING work
        // at the database level — duplicate external_tx_id values
        // are physically rejected by PostgreSQL, not just application logic.
        builder.HasIndex(t => t.ExternalTxId)
            .IsUnique();

        builder.Property(t => t.ExternalTxId)
            .HasColumnName("external_tx_id")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(t => t.NormalisedProviderTxId)
            .HasColumnName("normalised_provider_tx_id")
            .HasMaxLength(255);

        builder.Property(t => t.ProviderTransactionId)
            .HasColumnName("provider_transaction_id")
            .HasMaxLength(255);

        // Enum stored as a readable string, not an integer.
        // SELECT * FROM transactions WHERE status = 'Settled'
        // is far more useful than WHERE status = 1
        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.TransactionType)
            .HasColumnName("transaction_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.TransactionCategory)
            .HasColumnName("transaction_category")
            .HasMaxLength(50)
            .IsRequired();

        // PostgreSQL native array type — exactly what we decided in
        // Week 1 after seeing transaction_classification was an array
        // like ["Entertainment", "Games"] in the actual TrueLayer response.
        builder.Property(t => t.TransactionClassification)
            .HasColumnName("transaction_classification")
            .HasColumnType("text[]");

        builder.Property(t => t.ProviderCategoryDisplay)
            .HasColumnName("provider_category_display")
            .HasMaxLength(100);

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.MerchantName)
            .HasColumnName("merchant_name")
            .HasMaxLength(255);

        // decimal(18,2) — never float or double for money, as we
        // discussed. 18 total digits, 2 after the decimal point.
        // Signed: negative = debit, positive = credit.
        builder.Property(t => t.Amount)
            .HasColumnName("amount")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(t => t.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(t => t.TransactionDate)
            .HasColumnName("transaction_date")
            .IsRequired();

        builder.Property(t => t.RunningBalance)
            .HasColumnName("running_balance")
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.UserCategoryId)
            .HasColumnName("user_category_id");

        builder.Property(t => t.IsManuallyCategorised)
            .HasColumnName("is_manually_categorised")
            .HasDefaultValue(false);

        builder.Property(t => t.IsArchived)
            .HasColumnName("is_archived")
            .HasDefaultValue(false);

        // jsonb, not just json — PostgreSQL stores jsonb in a
        // decomposed binary format that's faster to query and index,
        // at a small cost to insert speed. For our read-heavy use
        // case (rarely inserting, occasionally querying raw payload
        // for debugging) jsonb is the right choice.
        builder.Property(t => t.RawPayload)
            .HasColumnName("raw_payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Foreign key relationship to Account.
        // Cascade: a transaction has no meaning without its account.
        // Deleting the account (e.g. user disconnects the bank)
        // should remove its transaction history along with it.
        // We're not exposing a navigation property back from
        // Transaction to Account on purpose — Transaction only
        // needs to know its AccountId, not load the full Account
        // every time. Keeps the entity lean.
        builder.HasOne<Account>()
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        // Foreign key relationship to Category, via UserCategoryId.
        // SetNull, not Cascade: if a user deletes a custom category,
        // their transaction history must survive — only the category
        // tag is removed. The three-tier fallback (provider
        // classification -> transaction_category) takes over for
        // display once UserCategoryId becomes null again.
        // WithMany() is empty deliberately — Category does not expose
        // a Transactions collection, since a category could be tagged
        // on thousands of transactions and we never want that loaded
        // as a side effect of loading a Category.
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(t => t.UserCategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Index on user_id for the denormalised query performance
        // pattern we discussed — "show me all this user's
        // transactions" never needs to join through accounts.
        builder.HasIndex(t => t.UserId);

        // Composite index — your most common query will be
        // "transactions for this user in this status,
        // ordered by date" (budget calculations always filter
        // Settled only). This index serves that exact query shape,
        // since PostgreSQL can use a composite index for any
        // query filtering on a left-to-right prefix of its columns.
        builder.HasIndex(t => new { t.UserId, t.Status, t.TransactionDate });
    }
}
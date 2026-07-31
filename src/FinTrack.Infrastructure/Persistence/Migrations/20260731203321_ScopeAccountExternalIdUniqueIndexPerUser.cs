using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ScopeAccountExternalIdUniqueIndexPerUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_accounts_external_account_id",
                table: "accounts");

            migrationBuilder.CreateIndex(
                name: "ix_accounts_external_account_id_user_id",
                table: "accounts",
                columns: new[] { "external_account_id", "user_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_accounts_external_account_id_user_id",
                table: "accounts");

            migrationBuilder.CreateIndex(
                name: "ix_accounts_external_account_id",
                table: "accounts",
                column: "external_account_id",
                unique: true);
        }
    }
}

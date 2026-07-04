using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinTrack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedSystemCategories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var categories = new[]
            {
        new { Id = Guid.NewGuid(), Name = "Groceries",     Colour = "#22C55E", Icon = "shopping-cart" },
        new { Id = Guid.NewGuid(), Name = "Transport",     Colour = "#3B82F6", Icon = "car" },
        new { Id = Guid.NewGuid(), Name = "Eating Out",    Colour = "#F97316", Icon = "utensils" },
        new { Id = Guid.NewGuid(), Name = "Entertainment", Colour = "#A855F7", Icon = "tv" },
        new { Id = Guid.NewGuid(), Name = "Bills",         Colour = "#EF4444", Icon = "file-text" },
        new { Id = Guid.NewGuid(), Name = "Shopping",      Colour = "#EC4899", Icon = "bag" },
        new { Id = Guid.NewGuid(), Name = "Personal Care", Colour = "#14B8A6", Icon = "heart" },
        new { Id = Guid.NewGuid(), Name = "Health",        Colour = "#06B6D4", Icon = "activity" },
        new { Id = Guid.NewGuid(), Name = "Travel",        Colour = "#F59E0B", Icon = "plane" },
        new { Id = Guid.NewGuid(), Name = "Other",         Colour = "#6B7280", Icon = "more-horizontal" },
    };

            foreach (var category in categories)
            {
                migrationBuilder.InsertData(
                    table: "categories",
                    columns: new[] { "id", "user_id", "name", "colour_hex", "icon", "is_system", "deleted_at" },
                    values: new object[] { category.Id, null, category.Name, category.Colour, category.Icon, true, null });
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM categories WHERE is_system = true");
        }
    }
}

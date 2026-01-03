using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace pokemon_discord_bot.Migrations
{
    /// <inheritdoc />
    public partial class addpokeballitems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "item_attributes");

            migrationBuilder.AddColumn<Dictionary<string, object>>(
                name: "attributes",
                table: "items",
                type: "jsonb",
                nullable: false);

            migrationBuilder.InsertData(
                table: "items",
                columns: new[] { "item_id", "attributes", "drop_chance", "name", "tradeable" },
                values: new object[,]
                {
                    { 1, new Dictionary<string, object> { ["CatchRateMultiplier"] = 1f }, 0.1f, "Pokeball", true },
                    { 2, new Dictionary<string, object> { ["CatchRateMultiplier"] = 1.2f }, 0.1f, "Great Ball", true },
                    { 3, new Dictionary<string, object> { ["CatchRateMultiplier"] = 1.5f }, 0.1f, "Ultra Ball", true },
                    { 4, new Dictionary<string, object> { ["CatchRateMultiplier"] = 1f }, 0.1f, "Love Ball", true },
                    { 5, new Dictionary<string, object> { ["GuaranteedCatch"] = true }, 0.1f, "Master Ball", true }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "items",
                keyColumn: "item_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "items",
                keyColumn: "item_id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "items",
                keyColumn: "item_id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "items",
                keyColumn: "item_id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "items",
                keyColumn: "item_id",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "attributes",
                table: "items");

            migrationBuilder.CreateTable(
                name: "item_attributes",
                columns: table => new
                {
                    attribute_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    item_id = table.Column<int>(type: "integer", nullable: false),
                    attributes = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_item_attributes", x => x.attribute_id);
                    table.ForeignKey(
                        name: "FK_item_attributes_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_item_attributes_item_id",
                table: "item_attributes",
                column: "item_id");
        }
    }
}

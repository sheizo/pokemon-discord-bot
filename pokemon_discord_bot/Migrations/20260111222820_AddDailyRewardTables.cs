using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace pokemon_discord_bot.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyRewardTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_rewards",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_rewards", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "daily_reward_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    claim_date = table.Column<DateOnly>(type: "date", nullable: false),
                    claimed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    daily_reward_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_reward_claims", x => x.id);
                    table.ForeignKey(
                        name: "FK_daily_reward_claims_daily_rewards_daily_reward_id",
                        column: x => x.daily_reward_id,
                        principalTable: "daily_rewards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "daily_reward_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    daily_reward_id = table.Column<int>(type: "integer", nullable: false),
                    item_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_reward_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_daily_reward_items_daily_rewards_daily_reward_id",
                        column: x => x.daily_reward_id,
                        principalTable: "daily_rewards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_daily_reward_items_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "daily_rewards",
                columns: new[] { "id", "is_active" },
                values: new object[] { 1, true });

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "item_id",
                keyValue: 1,
                column: "attributes",
                value: new Dictionary<string, object> { ["CatchRateMultiplier"] = 1f });

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "item_id",
                keyValue: 2,
                column: "attributes",
                value: new Dictionary<string, object> { ["CatchRateMultiplier"] = 1.2f });

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "item_id",
                keyValue: 3,
                column: "attributes",
                value: new Dictionary<string, object> { ["CatchRateMultiplier"] = 1.5f });

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "item_id",
                keyValue: 4,
                column: "attributes",
                value: new Dictionary<string, object> { ["CatchRateMultiplier"] = 1f });

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "item_id",
                keyValue: 5,
                column: "attributes",
                value: new Dictionary<string, object> { ["GuaranteedCatch"] = true });

            migrationBuilder.InsertData(
                table: "daily_reward_items",
                columns: new[] { "id", "daily_reward_id", "item_id", "quantity" },
                values: new object[] { 1, 1, 1, 5 });

            migrationBuilder.CreateIndex(
                name: "IX_daily_reward_claims_daily_reward_id",
                table: "daily_reward_claims",
                column: "daily_reward_id");

            migrationBuilder.CreateIndex(
                name: "IX_daily_reward_claims_user_id_claimed_at_utc",
                table: "daily_reward_claims",
                columns: new[] { "user_id", "claimed_at_utc" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_daily_reward_items_daily_reward_id",
                table: "daily_reward_items",
                column: "daily_reward_id");

            migrationBuilder.CreateIndex(
                name: "IX_daily_reward_items_item_id",
                table: "daily_reward_items",
                column: "item_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_reward_claims");

            migrationBuilder.DropTable(
                name: "daily_reward_items");

            migrationBuilder.DropTable(
                name: "daily_rewards");

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "item_id",
                keyValue: 1,
                column: "attributes",
                value: new Dictionary<string, object> { ["CatchRateMultiplier"] = 1f });

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "item_id",
                keyValue: 2,
                column: "attributes",
                value: new Dictionary<string, object> { ["CatchRateMultiplier"] = 1.2f });

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "item_id",
                keyValue: 3,
                column: "attributes",
                value: new Dictionary<string, object> { ["CatchRateMultiplier"] = 1.5f });

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "item_id",
                keyValue: 4,
                column: "attributes",
                value: new Dictionary<string, object> { ["CatchRateMultiplier"] = 1f });

            migrationBuilder.UpdateData(
                table: "items",
                keyColumn: "item_id",
                keyValue: 5,
                column: "attributes",
                value: new Dictionary<string, object> { ["GuaranteedCatch"] = true });
        }
    }
}

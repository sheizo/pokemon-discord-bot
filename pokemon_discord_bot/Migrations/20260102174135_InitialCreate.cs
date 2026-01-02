using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace pokemon_discord_bot.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:biome_type", "forest,water,grassland,cave,urban,mountain,desert")
                .Annotation("Npgsql:Enum:pokemon_gender", "male,female,genderless");

            migrationBuilder.CreateTable(
                name: "encounter_events",
                columns: table => new
                {
                    encounter_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    triggered_by = table.Column<long>(type: "bigint", nullable: false),
                    biome = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_encounter_events", x => x.encounter_id);
                });

            migrationBuilder.CreateTable(
                name: "items",
                columns: table => new
                {
                    item_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    drop_chance = table.Column<float>(type: "real", nullable: false),
                    tradeable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_items", x => x.item_id);
                });

            migrationBuilder.CreateTable(
                name: "pokemon_stats",
                columns: table => new
                {
                    stats_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    iv_hp = table.Column<short>(type: "smallint", nullable: false),
                    iv_atk = table.Column<short>(type: "smallint", nullable: false),
                    iv_def = table.Column<short>(type: "smallint", nullable: false),
                    iv_sp_atk = table.Column<short>(type: "smallint", nullable: false),
                    iv_sp_def = table.Column<short>(type: "smallint", nullable: false),
                    iv_speed = table.Column<short>(type: "smallint", nullable: false),
                    size = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pokemon_stats", x => x.stats_id);
                });

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

            migrationBuilder.CreateTable(
                name: "player_inventory",
                columns: table => new
                {
                    inventory_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    player_id = table.Column<long>(type: "bigint", nullable: false),
                    item_id = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_inventory", x => x.inventory_id);
                    table.ForeignKey(
                        name: "FK_player_inventory_items_item_id",
                        column: x => x.item_id,
                        principalTable: "items",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pokemons",
                columns: table => new
                {
                    pokemon_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    api_pokemon_id = table.Column<int>(type: "integer", nullable: false),
                    encounter_event_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    caught_by = table.Column<long>(type: "bigint", nullable: false),
                    owned_by = table.Column<long>(type: "bigint", nullable: false),
                    is_shiny = table.Column<bool>(type: "boolean", nullable: false),
                    gender = table.Column<int>(type: "integer", nullable: true),
                    caught_with = table.Column<int>(type: "integer", nullable: true),
                    pokemon_stats_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pokemons", x => x.pokemon_id);
                    table.ForeignKey(
                        name: "FK_pokemons_encounter_events_encounter_event_id",
                        column: x => x.encounter_event_id,
                        principalTable: "encounter_events",
                        principalColumn: "encounter_id");
                    table.ForeignKey(
                        name: "FK_pokemons_items_caught_with",
                        column: x => x.caught_with,
                        principalTable: "items",
                        principalColumn: "item_id");
                    table.ForeignKey(
                        name: "FK_pokemons_pokemon_stats_pokemon_stats_id",
                        column: x => x.pokemon_stats_id,
                        principalTable: "pokemon_stats",
                        principalColumn: "stats_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_item_attributes_item_id",
                table: "item_attributes",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "idx_inventory_player",
                table: "player_inventory",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_inventory_item_id",
                table: "player_inventory",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "idx_pokemon_owned_by",
                table: "pokemons",
                column: "owned_by");

            migrationBuilder.CreateIndex(
                name: "IX_pokemons_caught_with",
                table: "pokemons",
                column: "caught_with");

            migrationBuilder.CreateIndex(
                name: "IX_pokemons_encounter_event_id",
                table: "pokemons",
                column: "encounter_event_id");

            migrationBuilder.CreateIndex(
                name: "IX_pokemons_pokemon_stats_id",
                table: "pokemons",
                column: "pokemon_stats_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "item_attributes");

            migrationBuilder.DropTable(
                name: "player_inventory");

            migrationBuilder.DropTable(
                name: "pokemons");

            migrationBuilder.DropTable(
                name: "encounter_events");

            migrationBuilder.DropTable(
                name: "items");

            migrationBuilder.DropTable(
                name: "pokemon_stats");
        }
    }
}

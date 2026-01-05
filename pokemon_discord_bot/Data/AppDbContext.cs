using Microsoft.EntityFrameworkCore;
using Npgsql;
using pokemon_discord_bot.Data;

namespace PokemonBot.Data;

public class AppDbContext : DbContext
{
    public DbSet<PokemonStats> PokemonStats => Set<PokemonStats>();
    public DbSet<EncounterEvent> EncounterEvents => Set<EncounterEvent>();
    public DbSet<Pokemon> Pokemon => Set<Pokemon>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<PlayerInventory> PlayerInventory => Set<PlayerInventory>();


    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<PokemonGender>();
        modelBuilder.HasPostgresEnum<BiomeType>();

        //Create indexes
        modelBuilder.Entity<Pokemon>().HasIndex(e => e.OwnedBy).HasDatabaseName("idx_pokemon_owned_by");
        modelBuilder.Entity<PlayerInventory>().HasIndex(e => e.PlayerId).HasDatabaseName("idx_inventory_player");

        modelBuilder.Entity<Item>().HasData(
            new Item
            {
                ItemId = 1,
                Name = "Pokeball",
                DropChance = 0.1f,
                Tradeable = true,
                Attributes = new Dictionary<string, object> { { "CatchRateMultiplier", 1.00f } }
            },
            new Item
            {
                ItemId = 2,
                Name = "Great Ball",
                DropChance = 0.1f,
                Tradeable = true,
                Attributes = new Dictionary<string, object> { { "CatchRateMultiplier", 1.20f } }
            },
            new Item
            {
                ItemId = 3,
                Name = "Ultra Ball",
                DropChance = 0.1f,
                Tradeable = true,
                Attributes = new Dictionary<string, object> { { "CatchRateMultiplier", 1.50f } }
            },
            new Item
            {
                ItemId = 4,
                Name = "Love Ball",
                DropChance = 0.1f,
                Tradeable = true,
                Attributes = new Dictionary<string, object> { { "CatchRateMultiplier", 1.00f } }
            },
            new Item
            {
                ItemId = 5,
                Name = "Master Ball",
                DropChance = 0.1f,
                Tradeable = true,
                Attributes = new Dictionary<string, object> { { "GuaranteedCatch", true } }
            }
        );
        
        base.OnModelCreating(modelBuilder);
    }
}

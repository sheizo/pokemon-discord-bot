using Microsoft.EntityFrameworkCore;
using pokemon_discord_bot.Data;

namespace PokemonBot.Data;

public class AppDbContext : DbContext
{
    public DbSet<PokemonStats> PokemonStats => Set<PokemonStats>();
    public DbSet<EncounterEvent> EncounterEvents => Set<EncounterEvent>();
    public DbSet<Pokemon> Pokemon => Set<Pokemon>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<ItemAttributes> ItemAttributes => Set<ItemAttributes>();
    public DbSet<PlayerInventory> PlayerInventory => Set<PlayerInventory>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        String? connectionUrl = Environment.GetEnvironmentVariable("POKEMON_DISCORD_BOT_DB_URL");
        options.UseNpgsql( connectionUrl );
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<PokemonGender>();
        modelBuilder.HasPostgresEnum<BiomeType>();

        //Create indexes
        modelBuilder.Entity<Pokemon>().HasIndex(e => e.OwnedBy).HasDatabaseName("idx_pokemon_owned_by");
        modelBuilder.Entity<PlayerInventory>().HasIndex(e => e.PlayerId).HasDatabaseName("idx_inventory_player");
        
        base.OnModelCreating(modelBuilder);
    }
}

using Microsoft.EntityFrameworkCore;
using pokemon_discord_bot.Data;

namespace PokemonBot.Data;

public class AppDbContext : DbContext
{
    public DbSet<PokemonStats> PokemonStats => Set<PokemonStats>();
    public DbSet<EncounterEvent> EncounterEvents => Set<EncounterEvent>();
    public DbSet<Pokemon> Pokemon => Set<Pokemon>();
    public DbSet<Item> Items => Set<Item>();

    public DbSet<PlayerInventory> PlayerInventory => Set<PlayerInventory>();
    public DbSet<DailyReward> DailyRewards => Set<DailyReward>();
    public DbSet<DailyRewardItem> DailyRewardItems => Set<DailyRewardItem>();
    public DbSet<DailyRewardClaim> DailyRewardClaims => Set<DailyRewardClaim>();


    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<PokemonGender>();
        modelBuilder.HasPostgresEnum<BiomeType>();

        //Create indexes
        modelBuilder.Entity<Pokemon>().HasIndex(e => e.OwnedBy).HasDatabaseName("idx_pokemon_owned_by");
        modelBuilder.Entity<PlayerInventory>().HasIndex(e => e.PlayerId).HasDatabaseName("idx_inventory_player");

        SetupItems(modelBuilder);
        SetupDailyRewards(modelBuilder);
        
        base.OnModelCreating(modelBuilder);
    }

    private void SetupDailyRewards(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DailyRewardClaim>()
            .HasIndex(drc => new { drc.UserId, drc.ClaimedAtUtc })
            .IsDescending(false, true);  // UserId ASC, ClaimedAtUtc DESC for efficient "latest" queries

        modelBuilder.Entity<DailyRewardItem>()
            .HasOne(dri => dri.DailyReward)
            .WithMany(dr => dr.Items)
            .HasForeignKey(dri => dri.DailyRewardId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DailyRewardItem>()
            .HasOne(dri => dri.Item)
            .WithMany()  
            .HasForeignKey(dri => dri.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DailyRewardClaim>()
            .HasOne(drc => drc.DailyReward)
            .WithMany() 
            .HasForeignKey(drc => drc.DailyRewardId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DailyReward>().HasData(
            new DailyReward
            {
                DailyRewardId = 1,
                IsActive = true
            }
        );

        modelBuilder.Entity<DailyRewardItem>().HasData(
            new DailyRewardItem
            {
                DailyRewardItemId = 1,
                DailyRewardId = 1,
                ItemId = 1,
                Quantity = 5
            }
        );
    }

    private void SetupItems(ModelBuilder modelBuilder)
    {
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
    }

    public async Task<Pokemon> GetPokemonById(int pokemonId)
    {
        return await Pokemon
            .Include(p => p.PokemonStats)
            .Include(p => p.CaughtWithItem)
            .Include(p => p.EncounterEvent)
            .FirstAsync(p => p.PokemonId == pokemonId);
    }

    public async Task<List<Pokemon>> GetUserPokemonListAsync(ulong userId)
    {
        return await Pokemon
            .Include(p => p.PokemonStats)
            .Include(p => p.CaughtWithItem)
            .Where(p => p.OwnedBy == userId)
            .ToListAsync();
    }
}

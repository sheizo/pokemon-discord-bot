using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;
using pokemon_discord_bot.Data;

namespace pokemon_discord_bot.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Use the same connection logic as your Main, but load from env var directly (since no DI here)
            var connectionUrl = Environment.GetEnvironmentVariable("POKEMON_DISCORD_BOT_DB_URL");
            if (string.IsNullOrEmpty(connectionUrl))
                throw new InvalidOperationException("POKEMON_DISCORD_BOT_DB_URL environment variable is missing for design time.");

            var dataSource = new NpgsqlDataSourceBuilder(connectionUrl).EnableDynamicJson().Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(dataSource);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}

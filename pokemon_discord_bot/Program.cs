using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using PokemonBot.Data;

namespace pokemon_discord_bot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var services = new ServiceCollection();

            // Discord services
            services.AddSingleton(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            });
            services.AddSingleton<DiscordSocketClient>();
            services.AddSingleton<CommandService>();
            services.AddSingleton<CommandHandler>();
            services.AddSingleton<EncounterEventHandler>();

            // Build the data source once
            var connectionUrl = Environment.GetEnvironmentVariable("POKEMON_DISCORD_BOT_DB_URL");
            if (string.IsNullOrEmpty(connectionUrl))
                throw new InvalidOperationException("POKEMON_DISCORD_BOT_DB_URL environment variable is missing.");

            var dataSource = new NpgsqlDataSourceBuilder(connectionUrl).EnableDynamicJson().Build();
            services.AddSingleton(dataSource);
            
            services.AddDbContext<AppDbContext>(opts => opts.UseNpgsql(dataSource));

            var provider = services.BuildServiceProvider(validateScopes: true);

            var bot = new DiscordBotService(provider);
            await bot.StartAsync();

            // Keep the bot running
            await Task.Delay(-1);
        }
    }
}


using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PokemonBot.Data;

namespace pokemon_discord_bot
{
    public class DiscordBotService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly CommandHandler _commandHandler;

        private readonly string? _token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");

        public DiscordBotService(IServiceProvider provider)
        {
            _provider = provider;
            _client = provider.GetRequiredService<DiscordSocketClient>();
            _commands = provider.GetRequiredService<CommandService>();
            _commandHandler = provider.GetRequiredService<CommandHandler>();
        }

        public async Task StartAsync()
        {
            if (string.IsNullOrEmpty(_token))
                throw new InvalidOperationException("DISCORD_BOT_TOKEN environment variable is missing.");

            ApiPokemonData.Init();

            // Apply migrations on startup (using a temporary scope)
            using (var scope = _provider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await db.Database.MigrateAsync();
            }

            // Logging
            _client.Log += Log;
            _client.Ready += OnReady;

            // Install commands and hook message handler
            await _commandHandler.InstallCommandsAsync();

            // Login and start
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();
        }

        private static Task OnReady()
        {
            Console.WriteLine("Bot is ready!");
            return Task.CompletedTask;
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}

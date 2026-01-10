using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using pokemon_discord_bot.Data;
using PokemonBot.Data;
using System.Reflection;

namespace pokemon_discord_bot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _provider;

        // Retrieve client and CommandService instance via ctor
        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider provider)
        {
            _provider = provider;
            _commands = commands;
            _client = client;
        }

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived event into our command handler
            _client.MessageReceived += HandleCommandAsync;
            _client.InteractionCreated += HandleInteractionAsync;

            _commands.Log += (log) =>
            {
                Console.WriteLine(log);
                return Task.CompletedTask;
            };

            using var scope = _provider.CreateScope();
            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: scope.ServiceProvider);
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a system message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;


            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            bool hasPrefix = message.HasCharPrefix('p', ref argPos) ||
                            message.HasCharPrefix('P', ref argPos);

            if (!(hasPrefix ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                message.Author.IsBot)
                return;

            var context = new SocketCommandContext(_client, message);
            _ = Task.Run(async () =>
            {
                using var scope = _provider.CreateScope();
                await _commands.ExecuteAsync(
                    context: context,
                    argPos: argPos,
                    services: scope.ServiceProvider);
            });
        }

        private async Task HandleInteractionAsync(SocketInteraction interaction)
        {
            var scope = _provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            Random random = new Random();
            var guild = _client.GetGuild(interaction.GuildId ?? 0);
            var emoji = guild.Emotes.ElementAt(random.Next(guild.Emotes.Count));

            if (interaction is SocketMessageComponent component)
            {
                if (component.Data.CustomId.Contains("drop-button")) 
                {
                    int pokemonId = int.Parse(component.Data.CustomId.Substring("drop-button".Length));
                    Pokemon pokemon = await db.GetPokemonById(pokemonId);
                    
                    if (pokemon.CaughtBy != 0) return;

                    pokemon.CaughtBy = interaction.User.Id;
                    pokemon.OwnedBy = interaction.User.Id;
                    db.SaveChanges();

                    await component.RespondAsync($"{interaction.User.Mention} caught {pokemon.FormattedName} `{pokemon.IdBase36}` - IV: `{pokemon.PokemonStats.TotalIvPercent}%` {emoji}");
                } 
            }
        }
    }
}

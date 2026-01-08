using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
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
    }
}

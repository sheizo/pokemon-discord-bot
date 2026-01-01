using Discord;
using Discord.WebSocket;

namespace pokemon_discord_bot
{
    public class Program
    {
        private static DiscordSocketClient? _client;
        public static async Task Main()
        {
            _client = new DiscordSocketClient();
            _client.Log += Log;
            _client.MessageReceived += OnMessageReceived;
            _client.Ready += OnReady;

            String? token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private static Task OnReady()
        {
            return Task.CompletedTask;
        }

        private static Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private static Task OnMessageReceived(SocketMessage msg)
        {
            if (msg.Author.IsBot) return Task.CompletedTask;
            return Task.CompletedTask;
        }
    }
}

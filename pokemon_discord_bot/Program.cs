using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using PokemonBot.Data;

namespace pokemon_discord_bot
{
    public class Program
    {
        private static DiscordSocketClient _client = null!;
        private static AppDbContext _db = null!;

        public static async Task Main()
        {
            ApiPokemonData.Init();

            // Apply migrations automatically 
            _db = new AppDbContext();
            await _db.Database.MigrateAsync();

            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };

            _client = new DiscordSocketClient(config);
            _client.Log += Log;
            _client.MessageReceived += OnMessageReceived;
            _client.Ready += OnReady;

            string? token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");
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

        private static async Task OnMessageReceived(SocketMessage msg)
        {
            if (msg.Author.IsBot) return;
            if (msg.Content != "coninhas") return;

            ApiPokemon randomPokemon = ApiPokemonData.Instance.GetRandomPokemon();
            ApiPokemon randomPokemon2 = ApiPokemonData.Instance.GetRandomPokemon();
            ApiPokemon randomPokemon3 = ApiPokemonData.Instance.GetRandomPokemon();
            ApiPokemon randomPokemon4 = ApiPokemonData.Instance.GetRandomPokemon();
            ApiPokemon randomPokemon5 = ApiPokemonData.Instance.GetRandomPokemon();

            var bytes = await ImageEditor.CombineImagesAsync(new string[] { randomPokemon.Sprites.FrontShiny, randomPokemon2.Sprites.FrontShiny, randomPokemon3.Sprites.FrontShiny, randomPokemon4.Sprites.FrontDefault, randomPokemon5.Sprites.FrontDefault }, 2.0f);

            await msg.Channel.SendFileAsync(new FileAttachment(new MemoryStream(bytes), "coninhas.jpg"));
        }
    }
}

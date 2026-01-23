using Discord;
using Discord.Commands;
using pokemon_discord_bot.Data;
using pokemon_discord_bot.DiscordViews;
using pokemon_discord_bot.Services;

namespace pokemon_discord_bot.Modules
{
    public class PokemonViewModule : ModuleBase<SocketCommandContext>
    {
        private readonly AppDbContext _db;
        private readonly PokemonService _pokemonViewHandler;

        public PokemonViewModule(AppDbContext db, PokemonService pokemonViewHandler)
        {
            _db = db;
            _pokemonViewHandler = pokemonViewHandler;
        }

        [Command("view")]
        [Alias("v")]
        public async Task PokemonViewAsync(string? pokemonId = null)
        {
            var user = Context.User;

            try
            {
                Pokemon pokemon = await _pokemonViewHandler.GetPokemonAsync(user.Id, pokemonId, _db);

                var fileName = $"Pokemon{pokemon.FormattedName}{pokemon.IdBase36}.png";
                var embed = new PokemonView(fileName, pokemon).GetEmbed();

                //By default new pokemons will always have Default frame. (If check below only for older pokemons)
                string pokemonFramePath = "assets/frames/default_frame";

                if (pokemon.Frame != null) pokemonFramePath = pokemon.Frame.ImgPath;

                var attachment = await GetPokemonAttachment(fileName, pokemonFramePath, pokemon);

                await Context.Channel.SendFileAsync(attachment: attachment, embed: embed);
            }
            catch (Exception e)
            {
                await Context.Message.ReplyAsync("Invalid pokemon code");
            }
        }

        private async Task<FileAttachment> GetPokemonAttachment(string fileName, string? framePath, Pokemon pokemon)
        {
            var pokemonSize = pokemon.PokemonStats.Size;
            string pokemonSprite = pokemon.GetFrontSprite();

            var stream = await ImageEditor.GeneratePokemonWithFrame(pokemonSprite, framePath, pokemon, pokemonScaleFactor: pokemonSize);
            var fileAttachment = new FileAttachment(stream, fileName);

            return fileAttachment;
        }
    }
}

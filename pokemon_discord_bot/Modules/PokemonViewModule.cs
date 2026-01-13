

using Discord;
using Discord.Commands;
using pokemon_discord_bot.Data;
using pokemon_discord_bot.DiscordViews;
using PokemonBot.Data;

namespace pokemon_discord_bot.Modules
{
    public class PokemonViewModule : ModuleBase<SocketCommandContext>
    {
        private readonly AppDbContext _db;

        public PokemonViewModule(AppDbContext db)
        {
            _db = db;
        }

        [Command("view")]
        [Alias("v")]
        public async Task PokemonViewAsync(string? pokemonId = null)
        {
            var user = Context.User;
            var fileName = "pokemonview.png";


            if (pokemonId == null)
            {
                Pokemon lastPokemon = await _db.GetLastPokemonCaught(user.Id);
                var lastPokemonEmbed = new PokemonView(fileName, lastPokemon).GetEmbed();

                await Context.Channel.SendFileAsync(attachment: await GetPokemonAttachment(fileName, lastPokemon), embed: lastPokemonEmbed);
                return;
            }

            Pokemon pokemon = await _db.GetPokemonById(IdHelper.FromBase36(pokemonId));
            var embed = new PokemonView(fileName, pokemon).GetEmbed();

            await Context.Channel.SendFileAsync(attachment: await GetPokemonAttachment(fileName, pokemon), embed: embed);
        }

        private async Task<FileAttachment> GetPokemonAttachment(string fileName, Pokemon pokemon)
        {
            var pokemonSize = pokemon.PokemonStats.Size;
            string pokemonSprite = pokemon.GetFrontSprite();

            var stream = await ImageEditor.GenerateEmbedImageAsync(pokemonSprite, pokemon, pokemonScaleFactor: pokemonSize);
            var fileAttachment = new FileAttachment(stream, fileName);

            return fileAttachment;
        }
    }
}

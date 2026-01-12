using Discord.Commands;
using pokemon_discord_bot.Data;
using pokemon_discord_bot.DiscordViews;
using pokemon_discord_bot.Services;
using PokemonBot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pokemon_discord_bot.Modules
{
    [Group("collection")]
    [Alias("c", "col")]
    public class PokemonCollectionModule : ModuleBase<SocketCommandContext>
    {

        private readonly InteractionService _interactionService;
        private readonly AppDbContext _db;

        public PokemonCollectionModule(InteractionService interactionService, AppDbContext db)
        {
            _interactionService = interactionService;
            _db = db;
        }

        [Command("")]
        public async Task UserPokemonCollection(params string[] parameters)
        {
            var user = Context.User;

            List<Pokemon> pokemonList = await _db.GetUserPokemonListAsync(user.Id);

            if (pokemonList.Count == 0)
            {
                await Context.Channel.SendMessageAsync($"{user.Mention} You have no pokemons :(");
                return;
            }

            string? parameter = parameters.Length > 0 ? parameters[0] : null;
            string orderDescription = "id";
            if (parameter != null && parameter.StartsWith("o:")) orderDescription = parameter[2..].ToLower();

            char orderDirection = '>';
            if (parameter != null && parameter.EndsWith(">")) 
            {
                orderDescription = orderDescription[..^1];
                orderDirection = parameter.Last();
            } 
            else if (parameter != null && parameter.EndsWith("<"))
            {
                orderDescription = orderDescription[..^1];
                orderDirection = parameter.Last();
            }
            
            if (orderDescription == "id")
            {
                if (orderDirection == '>')
                    pokemonList = pokemonList.OrderByDescending(p => p.PokemonId).ToList();
                else
                    pokemonList = pokemonList.OrderBy(p => p.PokemonId).ToList();
            }
            else if (orderDescription == "iv")
            {
                if (orderDirection == '>')
                    pokemonList = pokemonList.OrderByDescending(p => p.PokemonStats.TotalIvPercent).ToList();
                else
                    pokemonList = pokemonList.OrderBy(p => p.PokemonStats.TotalIvPercent).ToList();
            }

            var collectionView = new CollectionView(user.Id, pokemonList);
            var embed = collectionView.GetEmbed();
            var component = collectionView.GetComponent();
            var message = await Context.Channel.SendMessageAsync(null, embed: embed, components: component);

            _interactionService.RegisterView(message.Id, collectionView);

            await Task.Delay(TimeSpan.FromMinutes(1));

            _interactionService.UnregisterView(message.Id);
        }
    }
}

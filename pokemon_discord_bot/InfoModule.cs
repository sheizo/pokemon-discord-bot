using Discord;
using Discord.Commands;
using Discord.WebSocket;
using pokemon_discord_bot.Data;
using PokemonBot.Data;

namespace pokemon_discord_bot
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private readonly EncounterEventHandler _encounterEventHandler;
        private readonly AppDbContext _db;

        public InfoModule(EncounterEventHandler encounterEventHandler, AppDbContext db)
        {
            _encounterEventHandler = encounterEventHandler;
            _db = db;
        }

        [Command("drop")]
        public async Task DropAsync()
        {   
            var user = Context.User;
            if (!_encounterEventHandler.CanUserTriggerEncounter(user.Id))
            {
                await Context.Channel.SendMessageAsync($"{user.Mention} ACALMA-TE CARALHO");
                return;
            }

            var encounter = await _encounterEventHandler.CreateRandomEncounterEvent(3, user.Id, _db);
            List<string> pokemonSprites = new List<string>();

            foreach(var pokemon in encounter.Pokemons)
            {

                //Getting pokemon sprite with specific gender
                pokemonSprites.Add(pokemon.GetFrontSprite());
            }

            var bytes = await ImageEditor.CombineImagesAsync(pokemonSprites, 2.0f);
            var fileName = "coninhas.png";
            var fileAttachment = new FileAttachment(new MemoryStream(bytes), fileName);
            var component = CardView.CreateDropView(fileName, Context.User.Mention, encounter);

            await Context.Channel.SendFileAsync(fileAttachment, components: component);
        }


        [Command("view")]
        public async Task PokemonViewAsync(string pokemonId)
        {
            var user = Context.User;

            Pokemon pokemon = await _db.GetPokemonById(IdHelper.FromBase36(pokemonId));
            var pokemonSize = pokemon.PokemonStats.Size;
            List<string> pokemonSprites = new List<string>() { pokemon.GetFrontSprite() };

            var bytes = await ImageEditor.CombineImagesAsync(pokemonSprites, pokemonSize);
            var fileName = "pokemonview.png";
            var fileAttachment = new FileAttachment(new MemoryStream(bytes), fileName);
            var component = CardView.CreatePokemonView(fileName, pokemon);

            await Context.Channel.SendFileAsync(fileAttachment, components: component);
        }

        [Command("inv")]
        public async Task UserPokemonCollection()
        {
            var user = Context.User;

            List<Pokemon> pokemonList = await _db.GetUserPokemonsAsync(user.Id);
            if (pokemonList.Count == 0) {
                await Context.Channel.SendMessageAsync($"{user.Mention} You have no pokemons :(");
                return;
            }

            (var embed, var component) = CardView.CreateCollectionView(pokemonList, user, 0);
            await Context.Channel.SendMessageAsync(null, embed: embed, components: component);
        }
    }

    // Create a module with the 'sample' prefix
    [Group("sample")]
    public class SampleModule : ModuleBase<SocketCommandContext>
    {
        // ~sample square 20 -> 400
        [Command("square")]
        [Summary("Squares a number.")]
        public async Task SquareAsync(
            [Summary("The number to square.")]
        int num)
        {
            // We can also access the channel from the Command Context.
            await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
        }

        [Command("userinfo")]
        [Summary
        ("Returns info about the current user, or the user parameter, if one passed.")]
        [Alias("user", "whois")]
        public async Task UserInfoAsync(
            [Summary("The (optional) user to get info from")]
        SocketUser user = null)
        {
            var userInfo = user ?? Context.Client.CurrentUser;
            await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
        }
    }


}

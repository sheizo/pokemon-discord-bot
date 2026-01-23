using Discord;
using Discord.Commands;
using pokemon_discord_bot.DiscordViews;
using pokemon_discord_bot.Helpers;
using pokemon_discord_bot.Services;
using pokemon_discord_bot.Data;

namespace pokemon_discord_bot.Modules
{
    [Group("encounter")]
    [Alias("e", "f", "find", "d", "drop")]
    public class PokemonEncounterModule : ModuleBase<SocketCommandContext>
    {
        private readonly InteractionService _interactionService;
        private readonly EncounterEventService _encounterEventHandler;
        private readonly AppDbContext _db;
        private readonly PokemonService _pokemonHandler;

        public PokemonEncounterModule(InteractionService interactionService, EncounterEventService encounterEventHandler, AppDbContext db, PokemonService pokemonHandler)
        {
            _interactionService = interactionService;
            _encounterEventHandler = encounterEventHandler;
            _db = db;
            _pokemonHandler = pokemonHandler;
        }

        [Command("")]
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

            foreach (var pokemon in encounter.Pokemons)
            {
                //Getting pokemon sprite with specific gender
                pokemonSprites.Add(pokemon.GetFrontSprite());
            }

            var bytes = await ImageEditor.CombineImagesAsync(pokemonSprites, 2.0f);
            var fileName = "coninhas.png";
            var fileAttachment = new FileAttachment(new MemoryStream(bytes), fileName);
            var encounterView = new EncounterView(_encounterEventHandler, encounter, user, _pokemonHandler, _interactionService, fileName);
            var component = encounterView.GetComponent();

            var message = await Context.Channel.SendFileAsync(fileAttachment, components: component);
            encounterView.SetMessage(message);

            _interactionService.RegisterView(
                message.Id,
                encounterView, 
                new InactivityTimer(TimeSpan.FromMinutes(3),
                    async () =>
                    {
                        await message.ModifyAsync(msg =>
                        {
                            msg.Components = encounterView.GetExpiredContent();
                        });
                        _interactionService.UnregisterView(message.Id);
                    }
                )
            );

        }
    }
}

using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using pokemon_discord_bot.Data;
using pokemon_discord_bot.Example;
using pokemon_discord_bot.Helpers;
using pokemon_discord_bot.Services;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace pokemon_discord_bot.DiscordViews
{
    public class EncounterView : IViewInteractable
    {
        private readonly EncounterEventService _encounterEventHandler;
        private readonly EncounterEvent _encounter;
        private readonly SocketUser _user;
        private readonly PokemonService _pokemonHandler;
        private readonly InteractionService _interactionService;

        private ConcurrentDictionary<ulong, int> _usersCatchingPokemons;
        private List<int> _caughtPokemonsId;
        private List<int> _fledPokemonsId;
        private IUserMessage _message;
        private string _fileName;

        public EncounterView(EncounterEventService encounterEventHandler, EncounterEvent encounter, SocketUser user, PokemonService pokemonHandler, InteractionService interactionService, string fileName)
        {
            _encounterEventHandler = encounterEventHandler;
            _encounter = encounter;
            _user = user;
            _pokemonHandler = pokemonHandler;
            _interactionService = interactionService;
            _fileName = fileName;

            _caughtPokemonsId = new List<int>();
            _fledPokemonsId = new List<int>();
            _usersCatchingPokemons = new ConcurrentDictionary<ulong, int>();
        }

        public MessageComponent GetComponent()
        {
            bool isLegendary = false;
            bool isMythical = false;

            List<ButtonBuilder> buttonList = new List<ButtonBuilder>();

            foreach (Pokemon pokemon in _encounter.Pokemons)
            {
                if (pokemon.ApiPokemon.Weight == 10) isLegendary = true;
                if (pokemon.ApiPokemon.Weight == 5) isMythical = true;

                string label = pokemon.FormattedName;
                if (pokemon.IsShiny) label = "\U0001F31F" + pokemon.FormattedName + "\U0001F31F";

                if (_caughtPokemonsId.Contains(pokemon.PokemonId))                 
                {
                    buttonList.Add(DiscordViewHelper.CreateViewButton($"drop-button{IdHelper.ToBase36(pokemon.PokemonId)}", $"{label} (Caught)", ButtonStyle.Success, true));
                    continue;
                }
                
                if (_fledPokemonsId.Contains(pokemon.PokemonId))
                {
                    buttonList.Add(DiscordViewHelper.CreateViewButton($"drop-button{IdHelper.ToBase36(pokemon.PokemonId)}", $"{label} (Fled)", ButtonStyle.Secondary, true));
                    continue;
                }

                if (IsUserCatchingPokemon(pokemon.PokemonId))
                    buttonList.Add(DiscordViewHelper.CreateViewButton($"drop-button{IdHelper.ToBase36(pokemon.PokemonId)}", $"(Catching) {label}", ButtonStyle.Danger, true));
                else 
                    buttonList.Add(DiscordViewHelper.CreateViewButton($"drop-button{IdHelper.ToBase36(pokemon.PokemonId)}", label));
            }

            return new ComponentBuilderV2()
                .WithTextDisplay($"{_user.Mention} found 3 pokemons!\n" +
                    $"{HasFoundSpecialPokemon(isLegendary, isMythical)}")
                .WithMediaGallery([
                    "attachment://" + _fileName
                ])
                .WithActionRow(buttonList)
                .Build();
        }

        private string HasFoundSpecialPokemon(bool isLegendary, bool isMythical)
        {
            if (isLegendary && isMythical) return "A **Mythical** and a **Legendary** Pokemon have appeared! \U0001F52E";
            if (isLegendary) return "A **Legendary** Pokemon has appeared! \U0001F52E";
            if (isMythical) return "A **Mythical** Pokemon has appeared! \U0001F52E";
            
            return "";
        }

        public void AddCaughtPokemon(Pokemon pokemon)
        {
            if (!_caughtPokemonsId.Contains(pokemon.PokemonId)) 
                _caughtPokemonsId.Add(pokemon.PokemonId);
        }

        public void AddFledPokemon(Pokemon pokemon)
        {
            if (!_fledPokemonsId.Contains(pokemon.PokemonId))
                _fledPokemonsId.Add(pokemon.PokemonId);
        }

        public void RemoveCatchingUser(ulong userId)
        {
            if (_usersCatchingPokemons.ContainsKey(userId))
                _usersCatchingPokemons.Remove(userId, out int value);
        }

        public async Task UpdateMessageAsync()
        {
            if (_message == null) return;
            
            var updatedComponents = GetComponent();
            await _message.ModifyAsync(msg => msg.Components = updatedComponents);
        }

        public void SetMessage(IUserMessage message)
        {
            _message = message;
        }

        public bool IsUserCatchingPokemon(int pokemonId)
        {
            foreach (var value in _usersCatchingPokemons.Values)
            {
                if (value == pokemonId)
                    return true;
            }

            return false;
        }
        public MessageComponent GetExpiredContent()
        {
            return new ComponentBuilderV2()
                .WithTextDisplay("*All remaining pokemons have vanished.*")
                .WithMediaGallery([
                    "attachment://" + _fileName
                ])
                .Build();
        }

        public async Task HandleInteraction(SocketMessageComponent component, IServiceProvider serviceProvider)
        {
            await component.DeferAsync(ephemeral: true);

            var messageId = component.Message.Id;
            var timer = _interactionService.TryGetViewTimer(messageId);
            timer.Reset();

            var db = serviceProvider.GetRequiredService<AppDbContext>();

            if (component.User.Id != _user.Id && _encounterEventHandler.CanDifferentUserClaimPokemon(_user.Id) == false)
            {
                await component.FollowupAsync("You cannot interact with this button!", ephemeral: true);
                return;
            }

            if (_usersCatchingPokemons.ContainsKey(component.User.Id))
            {
                await component.FollowupAsync("You're already catching a pokemon in this encounter!", ephemeral: true);
                return;
            }

            string pokemonId = component.Data.CustomId.Substring("drop-button".Length);
            Pokemon pokemon = await _pokemonHandler.GetPokemonAsync(component.User.Id, pokemonId, db);

            var allPokeballs = await db.Items
                .Where(i => Regex.IsMatch(i.Name, "Ball"))
                .ToListAsync();

            var allUserPokeballs = await db.PlayerInventory
                .Include(ui => ui.Item)
                .Where(ui => ui.PlayerId == component.User.Id && Regex.IsMatch(ui.Item.Name, "Ball"))
                .ToListAsync();

            if (allUserPokeballs.Count <= 0) 
            {
                await component.FollowupAsync("You don't have pokeballs! (use pdaily to get some)", ephemeral: true);
                return; 
            }

            //Cache user catching pokemon
            _usersCatchingPokemons[component.User.Id] = pokemon.PokemonId;

            await component.ModifyOriginalResponseAsync(msg => msg.Components = GetComponent());

            CatchView catchView = new CatchView(pokemon, component.User, _pokemonHandler, this);
            var catchViewComponent = await catchView.GetComponent(allUserPokeballs, allPokeballs);

            var message = await component.FollowupWithFileAsync(components: catchViewComponent.components, attachment: catchViewComponent.attachment);

            _interactionService.RegisterView(
                message.Id,
                catchView,
                new InactivityTimer(
                    TimeSpan.FromMinutes(5),
                    () =>
                    {
                       _interactionService.UnregisterView(message.Id);
                        return Task.CompletedTask;
                    }
                )
            );

        }
    }
}

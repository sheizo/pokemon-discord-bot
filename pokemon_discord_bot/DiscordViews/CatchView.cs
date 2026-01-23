using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using pokemon_discord_bot.Data;
using pokemon_discord_bot.Example;
using pokemon_discord_bot.Helpers;
using pokemon_discord_bot.Services;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace pokemon_discord_bot.DiscordViews
{
    public class CatchView : IViewInteractable
    {
        private readonly Pokemon _pokemon;
        private readonly SocketUser _user;
        private readonly PokemonService _pokemonHandler;
        private readonly EncounterView _encounterView;

        const float CATCH_PROBABILITY_NORMAL = 0.50f;
        const float CATCH_PROBABILITY_LEGENDARY = 0.1f;
        const float CATCH_PROBABILITY_MYTHICAL = 0.075f;

        private float fleeProbabilityNormal = 0.20f;
        private float fleeProbabilityLegendary = 0.10f;
        private float fleeProbabilityMythical = 0.08f;
        private float fleeRate = 1.15f;

        public CatchView(Pokemon pokemon, SocketUser user, PokemonService pokemonHandler, EncounterView encounterView)
        {
            _pokemon = pokemon;
            _user = user;
            _pokemonHandler = pokemonHandler;
            _encounterView = encounterView;
        }

        public async Task<(MessageComponent components, FileAttachment attachment)> GetComponent(List<PlayerInventory> userPokeballs, List<Item> allPokeballsSorted)
        {
            var fileName = $"catching_{IdHelper.ToBase36(_pokemon.PokemonId)}.png";
            var pokemonView = await ImageEditor.GeneratePokemonWithFrame(_pokemon.GetFrontSprite(), null, _pokemon, 1.0f);
            var fileAttachment = new FileAttachment(pokemonView, fileName);

            var userPokeballsQuantity = userPokeballs.ToDictionary(pi => pi.Item.ItemId, pi => pi.Quantity);

            var buttonList = new List<ButtonBuilder>(new ButtonBuilder[allPokeballsSorted.Count]);

            for (int i = 0; i < allPokeballsSorted.Count; i++)
            {
                var pokeball = allPokeballsSorted[i];

                string emoteString = DiscordViewHelper.PokeballEmotes.GetValueOrDefault(pokeball.Name, "");

                Emote? emote = null;
                if (emoteString != null && Emote.TryParse(emoteString, out var parsedEmote))
                    emote = parsedEmote;

                int quantity = userPokeballsQuantity.GetValueOrDefault(pokeball.ItemId, 0);
                bool disabled = quantity <= 0;

                buttonList[i] = DiscordViewHelper.CreateViewButton(
                    $"{pokeball.Name}catch-{_pokemon.PokemonId}",
                    $" {quantity}",
                    disabled ? ButtonStyle.Danger : ButtonStyle.Primary,
                    disabled,
                    emote: emote);
            }

            var builder = new ComponentBuilderV2()
            .WithTextDisplay($"{_user.Mention} is attempting to catch {_pokemon.FormattedName}\n" +
                $"Choose a Pokeball to catch it with:")
            .WithMediaGallery([
                "attachment://" + fileName
            ])
            .WithActionRow(buttonList)
            .Build();

            return (builder, fileAttachment);
        }

        private bool CaughtSuccessfully(Item pokeball)
        {
            if (pokeball.Name == "Master Ball") return true;

            var random = Random.Shared;

            var element = (JsonElement)pokeball.Attributes["CatchRateMultiplier"];

            float pokeballCatchRateMultiplier = (float) element.GetDouble();
            float catchRate;

            if (_pokemon.ApiPokemon.Weight == 5)
                catchRate = pokeballCatchRateMultiplier * CATCH_PROBABILITY_MYTHICAL;
            else if (_pokemon.ApiPokemon.Weight == 10)
                catchRate = pokeballCatchRateMultiplier * CATCH_PROBABILITY_LEGENDARY;
            else
                catchRate = pokeballCatchRateMultiplier * CATCH_PROBABILITY_NORMAL;

            return random.NextDouble() < 0.01;
        }

        private bool PokemonFled()
        {
            var random = Random.Shared;
            float fleeProbability;

            if (_pokemon.ApiPokemon.Weight == 5)
            {
                fleeProbability = fleeProbabilityMythical;
                fleeProbability *= fleeRate;
            }
                
            else if (_pokemon.ApiPokemon.Weight == 10)
                fleeProbability = fleeRate * fleeProbabilityLegendary;
            else
                fleeProbability = fleeRate * fleeProbabilityNormal;

            return random.NextDouble() < fleeProbability;
        }

        public async Task HandleInteraction(SocketMessageComponent component, IServiceProvider serviceProvider)
        {
            await component.DeferAsync(ephemeral: true);

            var db = serviceProvider.GetRequiredService<AppDbContext>();

            var allPokeballs = await db.Items
                .Where(i => Regex.IsMatch(i.Name, "Ball"))
                .OrderBy(i => i.ItemId)
                .ToListAsync();

            var currentUserPokeballs = await db.PlayerInventory
                .Include(ui => ui.Item)
                .Where(ui => ui.PlayerId == component.User.Id && Regex.IsMatch(ui.Item.Name, "Ball"))
                .ToListAsync();

            int position = component.Data.CustomId.IndexOf("catch-");
            string pokeballName = component.Data.CustomId.Substring(0, position);

            var userPokeball = currentUserPokeballs.FirstOrDefault(ui => ui.Item.Name == pokeballName);
            userPokeball.Quantity -= 10;

            if (userPokeball.Quantity <= 0)
            {
                currentUserPokeballs.Remove(userPokeball);
                db.Remove(userPokeball);
            }
                
            await db.SaveChangesAsync();

            Item pokeball = userPokeball.Item;
            
            if (CaughtSuccessfully(pokeball))
            {
                //Add pokemon to EncounterView CaughtPokemons List for button check
                _encounterView.AddCaughtPokemon(_pokemon);
                _encounterView.RemoveCatchingUser(component.User.Id);

                //Cache last pokemon caught by user who interacted with this component
                _pokemonHandler.SetLastPokemonOwned(component.User.Id, _pokemon);

                db.Pokemon.Update(_pokemon);

                _pokemon.CaughtBy = component.User.Id;
                _pokemon.OwnedBy = component.User.Id;

                await db.SaveChangesAsync();

                await component.Channel.SendMessageAsync($"{component.User.Mention} caught {_pokemon.FormattedName} `{_pokemon.IdBase36}` - IV: `{_pokemon.PokemonStats.TotalIvPercent}%` with a {pokeballName}!");

                //update EncounterView Component
                await _encounterView.UpdateMessageAsync();

                await component.DeleteOriginalResponseAsync();
                
                return;
            }
            else
            {
                if (PokemonFled())
                {
                    _encounterView.AddFledPokemon(_pokemon);
                    _encounterView.RemoveCatchingUser(component.User.Id);

                    await component.Channel.SendMessageAsync($"{component.User.Mention} The {_pokemon.FormattedName} fled!");
                    //update EncounterView Component
                    await _encounterView.UpdateMessageAsync();
                    await component.DeleteOriginalResponseAsync();
                    return;
                }

                if (currentUserPokeballs.Count <= 0)
                {
                    _encounterView.RemoveCatchingUser(component.User.Id);

                    await component.FollowupAsync("No pokeballs left!", ephemeral: true);
                    await _encounterView.UpdateMessageAsync();
                    await component.DeleteOriginalResponseAsync();

                    return;
                }

                var updatedComponents = await GetComponent(currentUserPokeballs, allPokeballs);

                await component.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Components = updatedComponents.components;
                });
            }
        }
    }
}

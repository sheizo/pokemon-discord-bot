using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using pokemon_discord_bot.Data;
using pokemon_discord_bot.Example;
using pokemon_discord_bot.Helpers;
using pokemon_discord_bot.Services;
using pokemon_discord_bot.Data;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace pokemon_discord_bot.DiscordViews
{
    public class CatchView : IViewInteractable
    {
        private readonly Pokemon _pokemon;
        private readonly SocketUser _user;
        private readonly PokemonHandler _pokemonHandler;
        private readonly EncounterView _encounterView;

        const float CATCH_PROBABILITY_NORMAL = 0.50f;
        const float CATCH_PROBABILITY_LEGENDARY = 0.1f;
        const float CATCH_PROBABILITY_MYTHICAL = 0.075f;

        public CatchView(Pokemon pokemon, SocketUser user, PokemonHandler pokemonHandler, EncounterView encounterView)
        {
            _pokemon = pokemon;
            _user = user;
            _pokemonHandler = pokemonHandler;
            _encounterView = encounterView;
        }

        public async Task<(MessageComponent components, FileAttachment attachment)> GetComponent(List<PlayerInventory> userPokeballs)
        {
            var fileName = $"catching_{IdHelper.ToBase36(_pokemon.PokemonId)}.png";
            var pokemonView = await ImageEditor.GeneratePokemonWithFrame(_pokemon.GetFrontSprite(), null, _pokemon, 1.0f);
            var fileAttachment = new FileAttachment(pokemonView, fileName);

            var sortedPokeballs = userPokeballs.OrderBy(p => p.ItemId).ToList();

            var buttonList = new List<ButtonBuilder>(new ButtonBuilder[userPokeballs.Count]);

            for (int i = 0; i < sortedPokeballs.Count; i++)
            {
                var pokeball = sortedPokeballs[i];

                string emoteString = DiscordViewHelper.PokeballEmotes.GetValueOrDefault(pokeball.Item.Name, "");

                Emote? emote = null;
                if (emoteString != null && Emote.TryParse(emoteString, out var parsedEmote))
                    emote = parsedEmote;

                bool disabled = pokeball.Quantity <= 0;

                buttonList[i] = DiscordViewHelper.CreateViewButton(
                    $"{pokeball.Item.Name}catch-{_pokemon.PokemonId}",
                    $" {pokeball.Quantity}",
                    disabled ? ButtonStyle.Danger : ButtonStyle.Primary,
                    disabled,
                    emote);
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

            return random.NextDouble() < 100;
        }

        public async Task HandleInteraction(SocketMessageComponent component, IServiceProvider serviceProvider)
        {
            await component.DeferAsync(ephemeral: true);

            DiscordViewHelper.ResetInactivityTimer();

            var db = serviceProvider.GetRequiredService<AppDbContext>();

            var allUserPokeballs = await db.PlayerInventory
                .Include(ui => ui.Item)
                .Where(ui => ui.PlayerId == component.User.Id && Regex.IsMatch(ui.Item.Name, "Ball"))
                .ToListAsync();

            int position = component.Data.CustomId.IndexOf("catch-");
            string pokeballName = component.Data.CustomId.Substring(0, position);

            var userPokeball = allUserPokeballs.FirstOrDefault(ui => ui.Item.Name == pokeballName);
            userPokeball.Quantity -= 10;

            if (userPokeball.Quantity <= 0)
            {
                db.Remove(userPokeball);
            }

            await db.SaveChangesAsync();

            Item pokeball = userPokeball.Item;
            
            if (CaughtSuccessfully(pokeball))
            {
                //Add pokemon to EncounterView CaughtPokemons List for button check
                _encounterView.AddCaughtPokemon(_pokemon);
                _encounterView.RemoveCatchingUser(component.User.Id);
                _encounterView.SetUserCatchingPokemon(false);

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
                DiscordViewHelper.StopInactivityTimer();
                
                return;
            }
            else
            {
                var updatedComponents = await GetComponent(allUserPokeballs);

                await component.ModifyOriginalResponseAsync(msg =>
                {
                    msg.Components = updatedComponents.components;
                });

                if (userPokeball.Quantity <= 0)
                {
                    await component.FollowupAsync(components: updatedComponents.components, ephemeral: true);
                }
            }
        }
    }
}

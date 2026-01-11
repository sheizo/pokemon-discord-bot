using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using pokemon_discord_bot.Data;
using pokemon_discord_bot.Example;
using PokemonBot.Data;
using System.Text;

namespace pokemon_discord_bot.DiscordViews
{
    public class CollectionView : IViewInteractable
    {
        private readonly ulong _userStartedId;
        private readonly List<Pokemon> _pokemonList;

        private const string PAG_BTN_FIRST_PAGE_ID = "pagination-button-first-page-";
        private const string PAG_BTN_PREVIOUS_PAGE_ID = "pagination-button-previous-page-";
        private const string PAG_BTN_NEXT_PAGE_ID = "pagination-button-next-page-";
        private const string PAG_BTN_LAST_PAGE_ID = "pagination-button-last-page-";
        private const int POKEMONS_PER_COLLECTION_PAGE = 10;

        private int _currentPageIndex = 0;

        public CollectionView(ulong userStartedId, List<Pokemon> pokemonList)
        {
            _userStartedId = userStartedId;
            _pokemonList = pokemonList;
        }

        public Embed GetEmbed()
        {
            StringBuilder stringBuilder = new StringBuilder();

            var range = new Range(_currentPageIndex * POKEMONS_PER_COLLECTION_PAGE, (_currentPageIndex + 1) * POKEMONS_PER_COLLECTION_PAGE);

            foreach (Pokemon pokemon in _pokemonList.Take(range))
            {
                string name = pokemon.FormattedName;
                if (pokemon.IsShiny) name = pokemon.FormattedName + " ✨";
                stringBuilder.AppendLine($"**{name}** - `{pokemon.IdBase36}` - IV: `{pokemon.PokemonStats.TotalIvPercent}%`");
            }

            return new EmbedBuilder()
                .WithColor(Color.DarkPurple)
                .WithDescription($"### <@{_userStartedId}>'s collection\n\n\n" + stringBuilder.ToString())
                .WithFooter($"Page: {_currentPageIndex + 1} \n({_pokemonList.Count} total pokemons)")
                .Build();
        }

        public MessageComponent GetComponent()
        {
            List<ButtonBuilder> buttonList = new List<ButtonBuilder>();
            buttonList.Add(DiscordViewHelper.CreateViewButton(PAG_BTN_FIRST_PAGE_ID, "\U000021A9"));
            buttonList.Add(DiscordViewHelper.CreateViewButton(PAG_BTN_PREVIOUS_PAGE_ID, "\U00002190"));
            buttonList.Add(DiscordViewHelper.CreateViewButton(PAG_BTN_NEXT_PAGE_ID, "\U00002192"));
            buttonList.Add(DiscordViewHelper.CreateViewButton(PAG_BTN_LAST_PAGE_ID, "\U000021AA"));

            return new ComponentBuilderV2()
                .WithActionRow(buttonList)
                .Build();
        }

        public async Task HandleInteraction(SocketMessageComponent component, IServiceProvider serviceProvider)
        {

            if (component.User.Id != _userStartedId)
            {
                await component.RespondAsync("You cannot interact with this button!", ephemeral: true);
                return;
            }

            int totalPageCount = _pokemonList.Count / POKEMONS_PER_COLLECTION_PAGE;
            if (_pokemonList.Count % POKEMONS_PER_COLLECTION_PAGE != 0) totalPageCount += 1;

            if (component.Data.CustomId == PAG_BTN_FIRST_PAGE_ID)
                _currentPageIndex = 0;
            else if (component.Data.CustomId == PAG_BTN_PREVIOUS_PAGE_ID && _currentPageIndex > 0)
                _currentPageIndex -= 1;
            else if (component.Data.CustomId == PAG_BTN_NEXT_PAGE_ID && _currentPageIndex < totalPageCount - 1)
                _currentPageIndex += 1;
            else if (component.Data.CustomId == PAG_BTN_LAST_PAGE_ID)
                _currentPageIndex = totalPageCount - 1;

            await component.UpdateAsync(msg =>
            {
                msg.Embed = GetEmbed();
                msg.Components = GetComponent();
            });
        }
    }
}

using Discord;
using Discord.WebSocket;
using pokemon_discord_bot.Data;
using pokemon_discord_bot.Example;
using pokemon_discord_bot.Helpers;
using System.Text;

namespace pokemon_discord_bot.DiscordViews
{
    public class InventoryView : IViewInteractable
    {
        private readonly ulong _userStartedId;
        private readonly List<PlayerInventory> _playerInventory;

        private const string PAG_BTN_FIRST_PAGE_ID = "pagination-button-first-page-";
        private const string PAG_BTN_PREVIOUS_PAGE_ID = "pagination-button-previous-page-";
        private const string PAG_BTN_NEXT_PAGE_ID = "pagination-button-next-page-";
        private const string PAG_BTN_LAST_PAGE_ID = "pagination-button-last-page-";
        private const int POKEMONS_PER_COLLECTION_PAGE = 10;

        private int _currentPageIndex = 0;

        public InventoryView(ulong userStartedId, List<PlayerInventory> playerInventory)
        {
            _userStartedId = userStartedId;
            _playerInventory = playerInventory;
        }

        public Embed GetEmbed()
        {
            StringBuilder stringBuilder = new StringBuilder();

            var range = new Range(_currentPageIndex * POKEMONS_PER_COLLECTION_PAGE, (_currentPageIndex + 1) * POKEMONS_PER_COLLECTION_PAGE);

            foreach (PlayerInventory playerInventory in _playerInventory.Take(range))
            {
                string emoteString = DiscordViewHelper.PokeballEmotes.GetValueOrDefault(playerInventory.Item.Name, "");

                Emote? emote = null;
                if (emoteString != null && Emote.TryParse(emoteString, out var parsedEmote))
                    emote = parsedEmote;

                string name = playerInventory.Item.Name;
                stringBuilder.AppendLine($"{emote} **{name}** - `{playerInventory.Quantity}`");
            }

            return new EmbedBuilder()
                .WithColor(Color.DarkPurple)
                .WithDescription($"### <@{_userStartedId}>'s inventory\n\n\n" + stringBuilder.ToString())
                .WithFooter($"Page: {_currentPageIndex + 1} \n({_playerInventory.Count} total items)")
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

            int totalPageCount = _playerInventory.Count / POKEMONS_PER_COLLECTION_PAGE;
            if (_playerInventory.Count % POKEMONS_PER_COLLECTION_PAGE != 0) totalPageCount += 1;

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

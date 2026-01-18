using Discord;

namespace pokemon_discord_bot.DiscordViews
{
    public class CommandsView
    {
        public MessageComponent GetComponent()
        {
            return new ComponentBuilderV2()
                .WithTextDisplay($"## **Commands** \n" +
                    $"### Encounter: `pencounter`, `pe`, `pfind`, `pf`, `pdrop`, `pd`\n" +
                    $"### Collection: `pcollection`, `pcol`, `pc`\n" +
                    $"### Inventory: `pinventory`, `pinv`, `pi`\n" +
                    $"### View Pokemon: `pview`, `pv`")
                .Build();
        }
    }
}

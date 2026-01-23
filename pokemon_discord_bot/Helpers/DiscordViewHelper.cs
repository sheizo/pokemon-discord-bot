using Discord;

namespace pokemon_discord_bot.Helpers
{
    public static class DiscordViewHelper
    {
        public static readonly Dictionary<string, string> PokeballEmotes = new()
        {
            { "Poke Ball", "<:poke_ball:1463071512096673864>" },
            { "Great Ball", "<:great_ball:1463071536368848958>" },
            { "Ultra Ball", "<:ultra_ball:1463071561920544873>" },
            { "Master Ball", "<:master_ball:1463071578626588839>" },
        };

        public static ButtonBuilder CreateViewButton(string customId, string label, ButtonStyle style = ButtonStyle.Primary, bool isDisabled = false, Emote? emote = null)
        {
            return new ButtonBuilder()
                .WithCustomId(customId)
                .WithLabel(label)
                .WithStyle(style)
                .WithDisabled(isDisabled)
                .WithEmote(emote);
        }
    }
}

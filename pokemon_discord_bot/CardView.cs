using Discord;
using pokemon_discord_bot.Data;
using System.Text;

namespace pokemon_discord_bot
{
    public class CardView
    {
        private static TextDisplayBuilder TextDisplay { get; set; } = null!;
        private static ButtonBuilder ButtonBuilder { get; set; } = null!;
        private static ActionRowBuilder ActionRowBuilder { get; set; } = null!;
        private static ComponentBuilderV2 ComponentBuilder { get; set; } = null!;

        public static MessageComponent CreateDropView(String fileName, string user, EncounterEvent encounter)
        {
            List<ButtonBuilder> buttonList = new List<ButtonBuilder>();

            foreach (Pokemon pokemon in encounter.Pokemons) 
            {
                string label = pokemon.FormattedName;
                if (pokemon.IsShiny) label = "\U0001F31F" + pokemon.FormattedName + "\U0001F31F";

                ButtonBuilder = new ButtonBuilder()
                    .WithCustomId("drop-button" + pokemon.PokemonId.ToString())
                    .WithLabel(label)
                    .WithStyle(ButtonStyle.Primary);

                buttonList.Add(ButtonBuilder);
            }

            TextDisplay = new TextDisplayBuilder().WithContent($"{user} found 3 pokemons!");
            ActionRowBuilder = new ActionRowBuilder().WithComponents(buttonList);
            ComponentBuilder = new ComponentBuilderV2()
                .WithTextDisplay(TextDisplay)
                .WithMediaGallery([
                    "attachment://" + fileName
                ])
                .WithActionRow(ActionRowBuilder);

            return ComponentBuilder.Build();
        }

        public static MessageComponent CreatePokemonView(String filename, Pokemon pokemon)
        {
            string pokemonStats =
                $"**TOTAL IV:** {pokemon.PokemonStats.TotalIvPercent}%\n" +
                $"**HP:** {pokemon.PokemonStats.IvHp}\n" +
                $"**ATK:** {pokemon.PokemonStats.IvAtk}\n" +
                $"**DEF:** {pokemon.PokemonStats.IvDef}\n" +
                $"**SPATK:** {pokemon.PokemonStats.IvSpAtk}\n" +
                $"**SPDEF:** {pokemon.PokemonStats.IvSpDef}\n" +
                $"**SPD:** {pokemon.PokemonStats.IvSpeed}\n" +
                $"**SIZE:** {pokemon.PokemonStats.Size}";

            var builder = new ComponentBuilderV2()
                .WithContainer(new ContainerBuilder()
                    .WithTextDisplay($"# {pokemon.FormattedName}")
                    .WithAccentColor(Color.DarkBlue)
                    .WithTextDisplay($"{pokemonStats}")
                    .WithMediaGallery([
                    "attachment://" + filename
                    ]))
                .Build();    

            return builder;
        }

        public static MessageComponent CreateInventoryView(List<Pokemon> pokemonList, ulong userId)
        {
            StringBuilder list = new StringBuilder();

            foreach (Pokemon pokemon in pokemonList.Take(10))
            {
                list.AppendLine($"`{pokemon.PokemonId}` - `{pokemon.PokemonStats.TotalIvPercent}`% - `{pokemon.FormattedName}`");
            }

            List<ButtonBuilder> buttonList = new List<ButtonBuilder>();
            buttonList.Add(CreatePaginationButton($"pagination-button-first-page-{userId}", "\U000021A9"));
            buttonList.Add(CreatePaginationButton($"pagination-button-previous-page-{userId}", "\U00002190"));
            buttonList.Add(CreatePaginationButton($"pagination-button-next-page-{userId}", "\U00002192"));
            buttonList.Add(CreatePaginationButton($"pagination-button-last-page-{userId}", "\U000021AA"));

            var builder = new ComponentBuilderV2()
                .WithTextDisplay(list.ToString())
                .WithActionRow(buttonList)
                .Build();

            return builder;
        }

        private static ButtonBuilder CreatePaginationButton(string customId, string label)
        {
            return new ButtonBuilder()
                .WithCustomId(customId)
                .WithLabel(label)
                .WithStyle(ButtonStyle.Primary);
        }
    }
}

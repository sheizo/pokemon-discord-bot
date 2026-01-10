using Discord;
using Discord.WebSocket;
using pokemon_discord_bot.Data;
using System.Reflection.Emit;
using System.Text;

namespace pokemon_discord_bot
{
    public class CardView
    {
        private static TextDisplayBuilder TextDisplay { get; set; } = null!;
        private static ButtonBuilder ButtonBuilder { get; set; } = null!;
        private static ActionRowBuilder ActionRowBuilder { get; set; } = null!;
        private static ComponentBuilderV2 ComponentBuilder { get; set; } = null!;

        private const int POKEMONS_PER_COLLECTION_PAGE = 10;

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

        public static (Embed, MessageComponent) CreateCollectionView(List<Pokemon> pokemonList, SocketUser user, int pageIndex)
        {
            StringBuilder list = new StringBuilder();

            var range = new Range(pageIndex * POKEMONS_PER_COLLECTION_PAGE, (pageIndex + 1) * POKEMONS_PER_COLLECTION_PAGE);
            foreach (Pokemon pokemon in pokemonList.Take(range))
            {
                string name = pokemon.FormattedName;
                if (pokemon.IsShiny) name = pokemon.FormattedName + " ✨";
                list.AppendLine($"**{name}** - `{pokemon.IdBase36}` - IV: `{pokemon.PokemonStats.TotalIvPercent}%`");
            }

            List<ButtonBuilder> buttonList = new List<ButtonBuilder>();
            buttonList.Add(CreatePaginationButton($"pagination-button-first-page-{user.Id}", "\U000021A9"));
            buttonList.Add(CreatePaginationButton($"pagination-button-previous-page-{user.Id}", "\U00002190"));
            buttonList.Add(CreatePaginationButton($"pagination-button-next-page-{user.Id}", "\U00002192"));
            buttonList.Add(CreatePaginationButton($"pagination-button-last-page-{user.Id}", "\U000021AA"));

            var embed = new EmbedBuilder()
                .WithColor(Color.DarkPurple)
                .WithDescription($"### {user.Mention}'s collection\n\n\n" + list.ToString())
                .Build();

            var component = new ComponentBuilderV2()
                .WithActionRow(buttonList)
                .Build();

            return (embed, component);
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

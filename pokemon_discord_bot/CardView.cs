using Discord;
using pokemon_discord_bot.Data;

namespace pokemon_discord_bot
{
    public class CardView
    {
        private static TextDisplayBuilder TextDisplay { get; set; } = null!;
        private static ButtonBuilder ButtonBuilder { get; set; } = null!;
        private static ActionRowBuilder ActionRowBuilder { get; set; } = null!;
        private static ComponentBuilderV2 ComponentBuilderV2 { get; set; } = null!;

        public static void SetTextDisplay(TextDisplayBuilder textDisplayBuilder)
        {
            TextDisplay = textDisplayBuilder;
        }
        public static void SetButtonBuilder(ButtonBuilder buttonBuilder)
        {
            ButtonBuilder = buttonBuilder;
        }
        public static void SetActionRow(ActionRowBuilder actionRowBuilder)
        {
            ActionRowBuilder = actionRowBuilder;
        }
        public static void SetComponentBuilderV2(ComponentBuilderV2 componentBuilderV2)
        {
            ComponentBuilderV2 = componentBuilderV2;
        }

        public static MessageComponent CreateDropView(String fileName, string user, EncounterEvent encounter)
        {
            List<ButtonBuilder> buttonList = new List<ButtonBuilder>();

            foreach (Pokemon pokemon in encounter.Pokemons) 
            {
                string label = pokemon.ApiPokemon.Name;
                if (pokemon.IsShiny) label = "\U0001F31F" + pokemon.ApiPokemon.Name + "\U0001F31F";

                SetButtonBuilder(new ButtonBuilder()
                    .WithCustomId(pokemon.PokemonId.ToString())
                    .WithLabel(label)
                    .WithStyle(ButtonStyle.Primary));

                buttonList.Add(ButtonBuilder);

                //buttonList.Add(new ButtonBuilder()
                //    .WithCustomId(pokemon.PokemonId.ToString())
                //    .WithLabel(label)
                //    .WithStyle(ButtonStyle.Primary));
            }

            SetTextDisplay(new TextDisplayBuilder().WithContent($"{user} found 3 pokemons!"));
            SetActionRow(new ActionRowBuilder().WithComponents(buttonList));
            SetComponentBuilderV2(new ComponentBuilderV2()
                .WithTextDisplay(TextDisplay)
                .WithMediaGallery([
                    "attachment://" + fileName
                ])
                .WithActionRow(ActionRowBuilder));

            var builder = ComponentBuilderV2.Build();
            //var builder = new ComponentBuilderV2()
            //    .WithTextDisplay(_textDisplay)
            //    .WithMediaGallery([
            //        "attachment://" + fileName
            //    ])
            //    .WithActionRow(buttonList)
            //    .Build();
            return builder;
        }

        public static MessageComponent CreatePokemonView(String filename, Pokemon pokemon)
        {
            string pokemonStats = 
                $"**HP:** {pokemon.PokemonStats.IvHp}\n" +
                $"**ATK:** {pokemon.PokemonStats.IvAtk}\n" +
                $"**DEF:** {pokemon.PokemonStats.IvDef}\n" +
                $"**SPATK:** {pokemon.PokemonStats.IvSpAtk}\n" +
                $"**SPDEF:** {pokemon.PokemonStats.IvSpDef}\n" +
                $"**SPD:** {pokemon.PokemonStats.IvSpeed}\n" +
                $"**SIZE:** {pokemon.PokemonStats.Size}";

            var builder = new ComponentBuilderV2()
                .WithContainer(new ContainerBuilder()
                    .WithTextDisplay($"# {pokemon.FormatPokemonName(pokemon.ApiPokemon.Name)}")
                    .WithAccentColor(Color.DarkBlue)
                    .WithTextDisplay($"{pokemonStats}")
                    .WithMediaGallery([
                    "attachment://" + filename
                    ]))
                .Build();    

            return builder;
        }
    }
}

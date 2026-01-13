using Discord;
using pokemon_discord_bot.Data;

namespace pokemon_discord_bot.DiscordViews
{
    public class PokemonView
    {
        private string _filename;
        private Pokemon _pokemon;

        public PokemonView(String filename, Pokemon pokemon)
        {
            _filename = filename;
            this._pokemon = pokemon;
        }

        public Embed GetEmbed()
        {
            string pokemonStats = new AnsiBuilder()
                .WithLine($"{_pokemon.PokemonStats.IvHp}", TextColor.Green).WithText(" HP")
                .WithLine($"{_pokemon.PokemonStats.IvAtk}", TextColor.Yellow).WithText(" ATK")
                .WithLine($"{_pokemon.PokemonStats.IvDef}", TextColor.Green).WithText(" DEF")
                .WithLine($"{_pokemon.PokemonStats.IvSpAtk}", TextColor.Green).WithText(" SPATK")
                .WithLine($"{_pokemon.PokemonStats.IvSpDef}", TextColor.Green).WithText(" SPDEF")
                .WithLine($"{_pokemon.PokemonStats.IvSpeed}", TextColor.Green).WithText(" SPEED")
                .WithBlankSpace()
                .WithLine("SIZE:").WithText($" {_pokemon.PokemonStats.Size}", TextColor.Green, bold: true)
                .Build();

            var builder = new EmbedBuilder()
                .WithColor((uint)new Random().Next(0, 16777216))
                .WithDescription($"### Pokemon stats\n" +
                    $"Owned by <@{_pokemon.OwnedBy}> \n\n" +
                    $"Name: **{_pokemon.FormattedName}** (`{_pokemon.IdBase36}`)\n" +
                    $"Total IV: **{_pokemon.PokemonStats.TotalIvPercent}%**\n")
                .WithImageUrl("attachment://" + _filename)
                .Build();
            return builder;
        }
    }
}

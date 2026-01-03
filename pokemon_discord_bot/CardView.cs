using Discord;
using Discord.WebSocket;
using System.Text;

namespace pokemon_discord_bot
{
    public class CardView
    {
        public static MessageComponent CreateDropView(String fileName, string user, uint size, ApiPokemon[] randomPokemons)
        {
            ButtonBuilder[] buttonList = new ButtonBuilder[randomPokemons.Length];

            for (int i = 0; i < randomPokemons.Length; i++)
            {
                buttonList[i] = new ButtonBuilder()
                    .WithCustomId(i.ToString())
                    .WithLabel(randomPokemons[i].Name)
                    .WithStyle(ButtonStyle.Primary);
            }

            var builder = new ComponentBuilderV2()
                .WithTextDisplay($"{user} has dropped 3 pokemons!")
                .WithMediaGallery([
                    "attachment://" + fileName
                ])
                .WithActionRow([
                    buttonList[0],
                    buttonList[1],
                    buttonList[2]
                ])
                .Build();

            return builder;
        }
    }
}

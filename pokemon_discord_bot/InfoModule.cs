using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pokemon_discord_bot
{
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;

    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        [Command("drop")]
        public async Task DropAsync()
        {
            ApiPokemon[] randomPokemons = ApiPokemonData.Instance.GetRandomPokemon(3);
            string[] randomPokemonsSprites = randomPokemons.Select(x => x.Sprites.FrontDefault).ToArray();

            var bytes = await ImageEditor.CombineImagesAsync(randomPokemonsSprites, 2.0f);
            var fileName = "coninhas.png";
            var fileAttachment = new FileAttachment(new MemoryStream(bytes), fileName);

            var component = CardView.CreateDropView(fileName, Context.User.Mention, 3, randomPokemons);

            await Context.Channel.SendFileAsync(fileAttachment, components: component);
        }
    }

    // Create a module with the 'sample' prefix
    [Group("sample")]
    public class SampleModule : ModuleBase<SocketCommandContext>
    {
        // ~sample square 20 -> 400
        [Command("square")]
        [Summary("Squares a number.")]
        public async Task SquareAsync(
            [Summary("The number to square.")]
        int num)
        {
            // We can also access the channel from the Command Context.
            await Context.Channel.SendMessageAsync($"{num}^2 = {Math.Pow(num, 2)}");
        }

        [Command("userinfo")]
        [Summary
        ("Returns info about the current user, or the user parameter, if one passed.")]
        [Alias("user", "whois")]
        public async Task UserInfoAsync(
            [Summary("The (optional) user to get info from")]
        SocketUser user = null)
        {
            var userInfo = user ?? Context.Client.CurrentUser;
            await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
        }
    }


}

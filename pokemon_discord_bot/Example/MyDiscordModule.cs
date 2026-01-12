using Discord.Commands;
using pokemon_discord_bot.Services;

namespace pokemon_discord_bot.Example
{
    public class MyDiscordModule : ModuleBase<SocketCommandContext>
    {
        private readonly InteractionService _interactionService;

        public MyDiscordModule(InteractionService interactionService)
        {
            _interactionService = interactionService;
        }

        [Command("counter")]
        public async Task CounterAsync()
        {
            var view = new MyDiscordView(Context.User.Id);
            var embed = view.GetEmbed();
            var component = view.GetComponent();
            var message = await Context.Channel.SendMessageAsync(embed: embed, components: component);
            _interactionService.RegisterView(message.Id, view);

            await Task.Delay(TimeSpan.FromMinutes(1));

            await message.ModifyAsync(msg =>
            {
                msg.Components = null;
                msg.Content = "This counter has expired.";
            });

            _interactionService.UnregisterView(message.Id);
        }
    }
}

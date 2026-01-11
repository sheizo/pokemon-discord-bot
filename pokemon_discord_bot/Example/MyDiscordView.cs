using Discord;
using Discord.WebSocket;

namespace pokemon_discord_bot.Example
{
    public class MyDiscordView : IViewInteractable
    {
        private readonly ulong _userStartedId; // Id of the user who created this view via command

        private const string INCREMENT_BUTTON_ID = "mydiscordview_" + "increment_button";

        private int _counter = 0;

        public MyDiscordView(ulong userStartedId)
        {
            _userStartedId = userStartedId;
        }

        public Embed GetEmbed()
        {
            return new EmbedBuilder()
                .WithTitle("My Discord View")
                .WithDescription($"Counter: {_counter}")
                .WithColor(Color.Blue)
                .Build();
        }

        public MessageComponent GetComponent()
        {
            return new ComponentBuilderV2()
                .WithActionRow(new ActionRowBuilder()
                    .WithButton(new ButtonBuilder()
                        .WithCustomId(INCREMENT_BUTTON_ID)
                        .WithLabel("Increment Me!")
                        .WithStyle(ButtonStyle.Primary)))
                .Build();
        }

        public async Task HandleInteraction(SocketMessageComponent component, IServiceProvider serviceProvider)
        {
            if (component.User.Id != _userStartedId)
            {
                await component.RespondAsync("You cannot interact with this button!", ephemeral: true);
                return;
            }

            if (component.Data.CustomId == INCREMENT_BUTTON_ID)
                _counter++;

            await component.UpdateAsync(msg =>
            {
                msg.Embed = GetEmbed();
                msg.Components = GetComponent();
            });
        }
    }
}

using Discord.Commands;
using pokemon_discord_bot.Data;
using pokemon_discord_bot.DiscordViews;
using pokemon_discord_bot.Services;

namespace pokemon_discord_bot.Modules
{
    public class DailyRewardModule : ModuleBase<SocketCommandContext>
    {
        private readonly AppDbContext _dbContext; 
        private readonly DailyRewardService _dailyRewardService;
        private readonly InteractionService _interactionService;

        public DailyRewardModule(AppDbContext dbContext, DailyRewardService dailyRewardService, InteractionService interactionService)
        {
            _dbContext = dbContext;
            _dailyRewardService = dailyRewardService;
            _interactionService = interactionService;
        }

        [Command("daily")]
        public async Task DailyAsync()
        {
            // Check if the user has already claimed their daily reward
            var canClaim = true; //await _dailyRewardService.CanClaimReward(Context.User.Id, _dbContext);

            if (canClaim)
            {
                try
                {
                    await _dailyRewardService.ClaimDailyReward(Context.User.Id, _dbContext);
                    await ReplyAsync($"{Context.User.Mention} Claimed his reward!");
                } 
                catch (Exception ex) {

                    await ReplyAsync($"{Context.User.Mention} There was an error claiming your daily reward.");
                }
            }
            else
            {
                await ReplyAsync($"{Context.User.Mention} You have already claimed your daily reward today. Try again tomorrow.");
            }
        }

        //TODO: MOVE TO SEPARATE MODULE
        [Command("inventory")]
        [Alias("inv", "i")]
        public async Task InventoryAsync()
        {
            var userId = Context.User.Id;

            List<PlayerInventory> playerInventories = await _dbContext.GetUserInventoryAsync(userId);

            var inventoryView = new InventoryView(userId, playerInventories);
            var embed = inventoryView.GetEmbed();
            var component = inventoryView.GetComponent();
            var message = await Context.Channel.SendMessageAsync(null, embed: embed, components: component);

            _interactionService.RegisterView(
                message.Id,
                inventoryView,
                new InactivityTimer(TimeSpan.FromMinutes(3),
                    () =>
                    {
                        _interactionService.UnregisterView(message.Id);
                        return Task.CompletedTask;
                    }
                )
            );

        }
    }
}

using Discord.Commands;
using pokemon_discord_bot.Services;
using PokemonBot.Data;
using System.Runtime.CompilerServices;
namespace pokemon_discord_bot.Modules
{
    public class DailyRewardModule : ModuleBase<SocketCommandContext>
    {
        private readonly AppDbContext _dbContext; 
        private readonly DailyRewardService _dailyRewardService;

        public DailyRewardModule(AppDbContext dbContext, DailyRewardService dailyRewardService)
        {
            _dbContext = dbContext;
            _dailyRewardService = dailyRewardService;
        }

        [Command("daily")]
        public async Task DailyAsync()
        {
            // Check if the user has already claimed their daily reward
            var canClaim = await _dailyRewardService.CanClaimReward(Context.User.Id, _dbContext);

            if (canClaim)
                await ReplyAsync($"{Context.User.Mention} Daily reward IS available");
            else
                await ReplyAsync($"{Context.User.Mention} You have already claimed your daily reward today. Please come back tomorrow!");
        }

        //TODO: MOVE TO SEPARATE MODULE
        [Command("inv")]
        public async Task InventoryAsync()
        {
            var userId = Context.User.Id;

            // Fetch the user's items from the database
            var userItems = await _dbContext.PlayerInventory
                .Include(ui => ui.Item)
                .Where(ui => ui.PlayerId == userId)
                .ToListAsync();

        }
    }
}

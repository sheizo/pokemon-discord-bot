using Microsoft.EntityFrameworkCore;
using pokemon_discord_bot.Data;
using PokemonBot.Data;
using System.Collections.Concurrent;

namespace pokemon_discord_bot.Services
{
    public class DailyRewardService
    {
        private ConcurrentDictionary<ulong, DailyRewardClaim> _claimCache;

        public DailyRewardService()
        {
            _claimCache = new ConcurrentDictionary<ulong, DailyRewardClaim>();
        }

        public async Task<bool> CanClaimReward(ulong userId, AppDbContext db)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            //Check cache first
            DailyRewardClaim? rewardClaim;
            if (_claimCache.ContainsKey(userId))
            {
                rewardClaim = _claimCache[userId];
                return rewardClaim.ClaimDate < today;
            }

             rewardClaim = await db.DailyRewardClaims.Where(c => c.UserId == userId)
                .OrderByDescending(c => c.ClaimedAtUtc)
                .FirstOrDefaultAsync();

            if (rewardClaim == null) return true;

            _claimCache[userId] = rewardClaim;
            return rewardClaim.ClaimDate < today;
        }

        public async Task ClaimDailyReward(ulong userId, AppDbContext db)
        {
            if (!await CanClaimReward(userId, db))
                throw new InvalidOperationException("Trying to Claim reward when it is on cooldown. Always call DailyRewardService.CanClaimReward first");
            
            //Get the current activate daily reward
            var dailyReward = await db.DailyRewards
                .Where(r => r.IsActive)
                .FirstOrDefaultAsync();

            if (dailyReward == null) 
                throw new InvalidOperationException("No active daily reward found to claim.");

            DailyRewardClaim dailyRewardClaim = new DailyRewardClaim
            {
                UserId = userId,
                DailyRewardId = dailyReward.DailyRewardId,
                ClaimDate = DateOnly.FromDateTime(DateTime.Today),
                ClaimedAtUtc = DateTime.UtcNow
            };
            await db.DailyRewardClaims.AddAsync(dailyRewardClaim);

            // Add items to player's inventory
            var rewardItems = await db.DailyRewardItems
                .Where(ri => ri.DailyRewardId == dailyReward.DailyRewardId)
                .ToListAsync();

            foreach (var rewardItem in rewardItems)
            {
                var playerInventory = await db.PlayerInventory
                    .Where(pi => pi.PlayerId == userId && pi.ItemId == rewardItem.ItemId)
                    .FirstOrDefaultAsync();
                if (playerInventory != null)
                {
                    playerInventory.Quantity += rewardItem.Quantity;
                    db.PlayerInventory.Update(playerInventory);
                }
                else
                {
                    playerInventory = new PlayerInventory
                    {
                        PlayerId = userId,
                        ItemId = rewardItem.ItemId,
                        Quantity = rewardItem.Quantity
                    };
                    await db.PlayerInventory.AddAsync(playerInventory);
                }
            }

            await db.SaveChangesAsync();
            _claimCache[userId] = dailyRewardClaim;
            return;
        }
    }
}

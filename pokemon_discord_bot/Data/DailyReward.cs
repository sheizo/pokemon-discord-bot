using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pokemon_discord_bot.Data
{
    [Table("daily_rewards")]
    public class DailyReward
    {
        [Key]
        [Column("id")]
        public int DailyRewardId { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        public ICollection<DailyRewardItem> Items { get; set; } = new List<DailyRewardItem>();
    }

    [Table("daily_reward_items")]
    public class DailyRewardItem
    {
        [Key]
        [Column("id")]
        public int DailyRewardItemId { get; set; }

        [Column("daily_reward_id")]
        public int DailyRewardId { get; set; }

        [Column("item_id")]
        public int ItemId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [ForeignKey(nameof(DailyRewardId))]
        public DailyReward DailyReward { get; set; } = null!;

        [ForeignKey(nameof(ItemId))]
        public Item Item { get; set; } = null!;
    }

    [Table("daily_reward_claims")]
    public class DailyRewardClaim
    {
        [Key]
        [Column("id")]
        public int DailyClaimId { get; set; }

        [Column("user_id", TypeName = "bigint")]
        public ulong UserId { get; set; }

        [Column("claim_date")]
        public DateOnly ClaimDate { get; set; }

        [Column("claimed_at_utc")]
        public DateTimeOffset ClaimedAtUtc { get; set; }

        [Column("daily_reward_id")]
        public int DailyRewardId { get; set; }

        [ForeignKey(nameof(DailyRewardId))]
        public DailyReward DailyReward { get; set; } = null!;
    }
}

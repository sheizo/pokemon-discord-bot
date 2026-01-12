using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pokemon_discord_bot.Data
{
    [Table("player_inventory")]
    public class PlayerInventory
    {
        [Key]
        [Column("inventory_id")]
        public int InventoryId { get; set; }

        [Column("player_id", TypeName = "bigint")]
        public ulong PlayerId { get; set; }

        [Column("item_id")]
        public int ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item Item { get; set; } = null!;

        [Column("quantity")]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    }
}

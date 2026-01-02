using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pokemon_discord_bot.Data
{
    [Table("items")]
    public class Item
    {
        [Key]
        [Column("item_id")]
        public int ItemId { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("drop_chance")]
        public float DropChance { get; set; } = 0.0f;

        [Column("tradeable")]
        public bool Tradeable { get; set; } = true;
    }
}

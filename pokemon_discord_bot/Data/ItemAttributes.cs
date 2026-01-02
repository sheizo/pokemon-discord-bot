using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pokemon_discord_bot.Data
{
    [Table("item_attributes")]
    public class ItemAttributes
    {
        [Key]
        [Column("attribute_id")]
        public int AttributeId { get; set; }

        [Column("item_id")]
        public int ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item Item { get; set; } = null!;

        [Column("attributes", TypeName = "jsonb")]
        [Required]
        public Dictionary<string, object> Attributes { get; set; } = new();
    }
}

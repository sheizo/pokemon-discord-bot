using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pokemon_discord_bot.Data
{
    public enum BiomeType
    {
        FOREST,
        WATER,
        GRASSLAND,
        CAVE,
        URBAN,
        MOUNTAIN,
        DESERT
    }

    [Table("encounter_events")]
    public class EncounterEvent
    {
        [Key]
        [Column("encounter_id")]
        public int EncounterId { get; set; }

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Column("triggered_by", TypeName = "bigint")]
        public long TriggeredBy { get; set; }

        [Column("biome")]
        public BiomeType? Biome { get; set; }
    }
}

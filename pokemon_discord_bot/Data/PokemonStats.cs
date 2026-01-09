using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pokemon_discord_bot.Data
{
    [Table("pokemon_stats")]
    public class PokemonStats
    {
        [Key]
        [Column("stats_id")]
        public int StatsId { get; set; }

        [Column("iv_hp", TypeName = "smallint")]
        [Range(0, 31)]
        public short IvHp { get; set; }

        [Column("iv_atk", TypeName = "smallint")]
        [Range(0, 31)]
        public short IvAtk { get; set; }

        [Column("iv_def", TypeName = "smallint")]
        [Range(0, 31)]
        public short IvDef { get; set; }

        [Column("iv_sp_atk", TypeName = "smallint")]
        [Range(0, 31)]
        public short IvSpAtk { get; set; }

        [Column("iv_sp_def", TypeName = "smallint")]
        [Range(0, 31)]
        public short IvSpDef { get; set; }

        [Column("iv_speed", TypeName = "smallint")]
        [Range(0, 31)]
        public short IvSpeed { get; set; }

        [Column("size")]
        public float Size { get; set; } = 1.0f;

        public int TotalIv => (IvHp + IvAtk + IvDef + IvSpAtk + IvSpDef + IvSpeed);

        public int TotalIvPercent => (TotalIv * 100) / (31 * 6);
    }
}

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace pokemon_discord_bot.Data
{
    public enum PokemonGender
    {
        MALE,
        FEMALE,
        GENDERLESS
    }

    [Table("pokemons")]
    public class Pokemon
    {
        [Key]
        [Column("pokemon_id")]
        public int PokemonId { get; set; }

        [Column("api_pokemon_id")]
        public int ApiPokemonId { get; set; }

        [Column("encounter_event_id")]
        public int? EncounterEventId { get; set; }

        [ForeignKey(nameof(EncounterEventId))]
        public EncounterEvent? EncounterEvent { get; set; }

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Column("caught_by", TypeName = "bigint")]
        public long CaughtBy { get; set; }

        [Column("owned_by", TypeName = "bigint")]
        public long OwnedBy { get; set; }

        [Column("is_shiny")]
        public bool IsShiny { get; set; } = false;

        [Column("gender")]
        public PokemonGender? Gender { get; set; }

        [Column("caught_with")]
        public int? CaughtWith { get; set; }

        [ForeignKey(nameof(CaughtWith))]
        public Item? CaughtWithItem { get; set; }

        [Column("pokemon_stats_id")]
        public int PokemonStatsId { get; set; }

        [ForeignKey(nameof(PokemonStatsId))]
        public PokemonStats PokemonStats { get; set; } = null!;
    }
}

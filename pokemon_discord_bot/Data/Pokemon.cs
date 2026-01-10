using Microsoft.EntityFrameworkCore;
using PokemonBot.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

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
        public ulong CaughtBy { get; set; }

        [Column("owned_by", TypeName = "bigint")]
        public ulong OwnedBy { get; set; }

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

        //----------------------------------------------------------------------------------------------

        public string IdBase36 => IdHelper.ToBase36(PokemonId);

        public ApiPokemon ApiPokemon => ApiPokemonData.Instance.GetPokemon(ApiPokemonId);
        public string FormattedName {
            get
            {
                if (string.IsNullOrEmpty(ApiPokemon.Name)) return "MISSING NAME";
                return char.ToUpper(ApiPokemon.Name[0]) + ApiPokemon.Name.Substring(1);
            }
        }

        public string GetFrontSprite()
        {
            //check if pokemon has a female gender through sprite
            if (Gender == PokemonGender.FEMALE && ApiPokemon.Sprites.FrontFemale != null)
            {
                if (IsShiny)
                {
                    return ApiPokemon.Sprites.FrontShinyFemale;
                }
                return ApiPokemon.Sprites.FrontFemale;
            }

            //if not return default (male)
            if (IsShiny) return ApiPokemon.Sprites.FrontShiny;
            else return ApiPokemon.Sprites.FrontDefault;
        }
    }
}

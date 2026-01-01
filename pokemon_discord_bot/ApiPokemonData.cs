using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.IO.Enumeration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace pokemon_discord_bot
{
    internal class ApiPokemonData
    {
        private const string FILENAME = "all_pokemons.json";
        private static ApiPokemonData instance = null;
        private static readonly object padlock = new object();
        public Dictionary<int, ApiPokemon> Pokemons { get; private set; }

        public static ApiPokemonData Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new ApiPokemonData();
                    }
                    return instance;
                }
            }
        }

        public static void Init()
        {
            instance = new ApiPokemonData();
            var path = Path.Combine(AppContext.BaseDirectory, FILENAME);
            var json = File.ReadAllText(path);
            instance.Pokemons = JsonSerializer.Deserialize<Dictionary<int, ApiPokemon>>(json)!;
        }

        public ApiPokemon GetRandomPokemon()
        {
            return Pokemons.Values.ElementAt(new Random().Next(0, Pokemons.Count));
        }
    }

    public class ApiPokemon
    {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("id")] public uint Id { get; set; }
        [JsonPropertyName("types")] public List<string> Types { get; set; }
        [JsonPropertyName("sprites")] public Sprites Sprites { get; set; }
        [JsonPropertyName("weight")] public uint Weight { get; set; }
    }
    public class Sprites
    {
        [JsonPropertyName("front_default")] public string FrontDefault { get; set; }
        [JsonPropertyName("front_female")] public string FrontFemale { get; set; }
        [JsonPropertyName("front_shiny")] public string FrontShiny { get; set; }
        [JsonPropertyName("front_shiny_female")] public string FrontShinyFemale { get; set; }
    }
}

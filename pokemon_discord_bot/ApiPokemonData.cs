using pokemon_discord_bot.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace pokemon_discord_bot
{
    internal class ApiPokemonData
    {
        private const string FILENAME = "all_pokemons.json";
        private static ApiPokemonData instance = null!;
        private static readonly object padlock = new object();
        public Dictionary<int, ApiPokemon> Pokemons { get; private set; } = new Dictionary<int, ApiPokemon>();

        private uint _totalWeight = 0;

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

        public ApiPokemonData()
        {
            var path = Path.Combine(AppContext.BaseDirectory, FILENAME);
            var json = File.ReadAllText(path);
            Pokemons = JsonSerializer.Deserialize<Dictionary<int, ApiPokemon>>(json)!;

            if (Pokemons == null) return;

            foreach (ApiPokemon pokemon in Pokemons.Values)
                _totalWeight += pokemon.Weight;
        }

        public ApiPokemon GetPokemon(int id)
        {
            if (!Pokemons.ContainsKey(id)) return null!;
            return Pokemons[id];
        }

        public List<ApiPokemon> GetRandomPokemon(uint quantity)
        {
            List<ApiPokemon> randomPokemons = new List<ApiPokemon>();

            for (int i = 0; i < quantity; i++)
                randomPokemons.Add(GetRandomPokemon());

            return randomPokemons;
        }

        public ApiPokemon GetRandomPokemon()
        {
            int random = new Random().Next(0, (int)_totalWeight);
            foreach (ApiPokemon pokemon in instance.Pokemons.Values)
            {
                random -= (int)pokemon.Weight;
                if (random < 0) return pokemon;
            }

            return null!;
        }

        public static PokemonGender GetRandomPokemonGender(Pokemon pokemon) 
        {
            return GetRandomPokemonGender(pokemon.ApiPokemon);
        }

        public static PokemonGender GetRandomPokemonGender(ApiPokemon apiPokemon)
        {
            if (apiPokemon.GenderRate == -1) return PokemonGender.GENDERLESS;

            double femaleChance = apiPokemon.GenderRate / 8d;
            return new Random().NextDouble() > femaleChance ? PokemonGender.MALE : PokemonGender.FEMALE;
        }
    }

    public class ApiPokemon
    {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("id")] public uint Id { get; set; }
        [JsonPropertyName("types")] public List<string> Types { get; set; }
        [JsonPropertyName("sprites")] public Sprites Sprites { get; set; }
        [JsonPropertyName("weight")] public uint Weight { get; set; }
        [JsonPropertyName("gender_rate")] public sbyte GenderRate { get; set; }
    }
    public class Sprites
    {
        [JsonPropertyName("front_default")] public string FrontDefault { get; set; }
        [JsonPropertyName("front_female")] public string FrontFemale { get; set; }
        [JsonPropertyName("front_shiny")] public string FrontShiny { get; set; }
        [JsonPropertyName("front_shiny_female")] public string FrontShinyFemale { get; set; }
    }
}

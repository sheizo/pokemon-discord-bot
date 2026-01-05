using Microsoft.Extensions.DependencyInjection;
using pokemon_discord_bot.Data;
using PokemonBot.Data;

namespace pokemon_discord_bot
{
    public class EncounterEventHandler
    {
        private const uint DROP_COOLDOWN_SECONDS = 0;
        private const uint CLAIM_COOLDOWN_SECONDS = 5;
        private const double SHINY_CHANCE = 1d/10d;
        private const int MIN_POKEMON_SIZE = 1;
        private const int MAX_POKEMON_SIZE = 5;

        private Dictionary<ulong, DateTimeOffset> _lastTriggerTime;
        private Dictionary<ulong, DateTimeOffset> _lastClaimTime;

        public EncounterEventHandler() {
            _lastTriggerTime = new Dictionary<ulong, DateTimeOffset>();
            _lastClaimTime = new Dictionary<ulong, DateTimeOffset>();
        }

        public async Task<EncounterEvent> CreateRandomEncounterEvent(int pokemonAmount, ulong userId, AppDbContext db)
        {
            if (!CanUserTriggerEncounter(userId)) 
                throw new Exception("Tried to create random encounter event when user is on cooldown. Should always call CanUserTriggerEncounter first");

            EncounterEvent encounterEvent = new EncounterEvent();
            encounterEvent.Biome = BiomeType.FOREST;
            encounterEvent.TriggeredBy = (long) userId;

            //Below line needs an EncounterEvent in the DB present
            List<Pokemon> pokemons = await CreateRandomPokemons(pokemonAmount, encounterEvent, db);
            await db.SaveChangesAsync();

            _lastTriggerTime[userId] = DateTimeOffset.UtcNow;

            encounterEvent.Pokemons = pokemons;
            return encounterEvent;
        }

        private async Task<List<Pokemon>> CreateRandomPokemons(int pokemonAmount, EncounterEvent encounterEvent, AppDbContext db)
        {
            ApiPokemon[] randomPokemons = ApiPokemonData.Instance.GetRandomPokemon(3);
            List<Pokemon> pokemons = new List<Pokemon>();

            foreach (ApiPokemon apiPokemon in randomPokemons)
            {
                Random random = new Random();

                Pokemon pokemon = new Pokemon();
                pokemon.ApiPokemonId = (int) apiPokemon.Id;
                pokemon.EncounterEvent = encounterEvent;
                pokemon.IsShiny = Math.Round(random.NextDouble(), 5) < SHINY_CHANCE;
                var values = Enum.GetValues<PokemonGender>();
                pokemon.Gender = (PokemonGender)values.GetValue(random.Next(values.Length)); //TODO: SHOULD BE RANDOMLY MALE/FEMALE/GENDERLESS (depending on whicih pokemon)
                pokemon.PokemonStats = new PokemonStats()
                {
                    IvAtk = (short)(random.NextInt64(0, 31) + 1),
                    IvDef = (short)(random.NextInt64(0, 31) + 1),
                    IvHp = (short)(random.NextInt64(0, 31) + 1),
                    IvSpAtk = (short)(random.NextInt64(0, 31) + 1),
                    IvSpDef = (short)(random.NextInt64(0, 31) + 1),
                    IvSpeed = (short)(random.NextInt64(0, 31) + 1),
                    Size = (float)(MIN_POKEMON_SIZE + (Math.Round(random.NextDouble(), 2) * (MAX_POKEMON_SIZE - MIN_POKEMON_SIZE)))
                };

                pokemons.Add(pokemon);
            }

            await db.Pokemon.AddRangeAsync(pokemons);
            return pokemons;
        }

        public bool CanUserTriggerEncounter(ulong userId)
        {
            if (!_lastTriggerTime.ContainsKey(userId)) return true;

            //Check if user is on cooldown
            DateTimeOffset lastTrigger = _lastTriggerTime[userId];
            var elapsed = DateTimeOffset.UtcNow - lastTrigger;
            return elapsed.TotalSeconds > DROP_COOLDOWN_SECONDS;
        }
    }
}

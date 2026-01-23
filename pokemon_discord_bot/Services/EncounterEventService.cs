using pokemon_discord_bot.Data;
using System.Collections.Concurrent;

namespace pokemon_discord_bot.Services
{
    public class EncounterEventService
    {
        private const uint DROP_COOLDOWN_SECONDS = 5;
        private const uint CLAIM_COOLDOWN_SECONDS = 5;
        private const uint OTHER_USER_CATCHABLE_COOLDOWN = 5;
        private const double SHINY_CHANCE = 1d/10d;
        private const float MIN_POKEMON_SIZE = 1.0f;
        private const float MAX_POKEMON_SIZE = 3.0f;

        private ConcurrentDictionary<ulong, DateTimeOffset> _lastTriggerTime;
        private ConcurrentDictionary<ulong, DateTimeOffset> _lastClaimTime;

        public EncounterEventService() {
            _lastTriggerTime = new ConcurrentDictionary<ulong, DateTimeOffset>();
            _lastClaimTime = new ConcurrentDictionary<ulong, DateTimeOffset>();
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
            List<ApiPokemon> randomPokemons = ApiPokemonData.Instance.GetRandomPokemon(3);
            List<Pokemon> pokemons = new List<Pokemon>();

            foreach (ApiPokemon apiPokemon in randomPokemons)
            {
                Random random = new Random();

                Pokemon pokemon = new Pokemon();
                pokemon.ApiPokemonId = (int) apiPokemon.Id;
                pokemon.EncounterEvent = encounterEvent;
                pokemon.IsShiny = random.NextDouble() < SHINY_CHANCE;
                pokemon.Gender = ApiPokemonData.GetRandomPokemonGender(pokemon);
                pokemon.PokemonStats = new PokemonStats()
                {
                    IvAtk = (short)random.NextInt64(0, 32),
                    IvDef = (short)random.NextInt64(0, 32),
                    IvHp = (short)random.NextInt64(0, 32),
                    IvSpAtk = (short)random.NextInt64(0, 32),
                    IvSpDef = (short)random.NextInt64(0, 32),
                    IvSpeed = (short)random.NextInt64(0, 32),
                    Size = (float)Math.Round(MIN_POKEMON_SIZE + random.NextDouble() * (MAX_POKEMON_SIZE - MIN_POKEMON_SIZE), 2)
                };

                pokemons.Add(pokemon);
            }

            await db.Pokemon.AddRangeAsync(pokemons);
            return pokemons;
        }

        public double GetTimeSinceLastTrigger(ulong userId)
        {
            DateTimeOffset lastTrigger = _lastTriggerTime[userId];
            var elapsed = DateTimeOffset.UtcNow - lastTrigger;

            return elapsed.TotalSeconds;
        }

        public bool CanUserTriggerEncounter(ulong userId)
        {
            if (!_lastTriggerTime.ContainsKey(userId)) return true;

            return GetTimeSinceLastTrigger(userId) > DROP_COOLDOWN_SECONDS;
        }

        public bool CanDifferentUserClaimPokemon(ulong userId)
        {
            if (!_lastTriggerTime.ContainsKey(userId)) return true;

            return GetTimeSinceLastTrigger(userId) > OTHER_USER_CATCHABLE_COOLDOWN;
        }
    }
}

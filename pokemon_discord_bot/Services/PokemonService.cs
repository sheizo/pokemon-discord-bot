using pokemon_discord_bot.Data;
using pokemon_discord_bot.Helpers;
using System.Collections.Concurrent;

namespace pokemon_discord_bot.Services
{
    public class PokemonService
    {
        private ConcurrentDictionary<ulong, Pokemon> _lastPokemonOwned;

        public PokemonService() 
        {
            _lastPokemonOwned = new ConcurrentDictionary<ulong, Pokemon>();
        }

        public async Task<Pokemon> GetPokemonAsync(ulong userId, string? pokemonId, AppDbContext db)
        {
            if (pokemonId != null)
                return await db.GetPokemonById(IdHelper.FromBase36(pokemonId));

            if (!_lastPokemonOwned.ContainsKey(userId)) 
                throw new Exception("Bot has restarted, no Pokemon cached");

            return _lastPokemonOwned[userId];
        }

        public void SetLastPokemonOwned(ulong userId, Pokemon pokemon)
        {
            _lastPokemonOwned[userId] = pokemon;
        }
    }
}

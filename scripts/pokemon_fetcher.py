import aiohttp
import asyncio
import json
from pathlib import Path

from typing import Dict, Any

BASE_URL = "https://pokeapi.co/api/v2"
SPRITE_NAMES = ["front_default", "front_shiny", "front_female", "front_shiny_female"]
CONCURRENT = 50
path = Path().resolve().parent
file_path = path/"pokemon_discord_bot"/"all_pokemons.json"

async def fetch_pokemon(session: aiohttp.ClientSession, name_or_id: str) -> Dict[str, Any]:
    """Async fetch pokemon + species."""
    tasks = [
        session.get(f"{BASE_URL}/pokemon/{name_or_id}"),
        session.get(f"{BASE_URL}/pokemon-species/{name_or_id}")
    ]
    pokemon_resp, species_resp = await asyncio.gather(*tasks)
    
    pokemon = await pokemon_resp.json()
    species = await species_resp.json()
    
    data = {
        "name": pokemon["name"],
        "id": pokemon["id"],
        "types": [t["type"]["name"] for t in pokemon["types"]],
        "sprites": {k: v for k, v in pokemon["sprites"].items() if k in SPRITE_NAMES},
        "weight": 100
    }
    
    if species["is_mythical"]:
        data["weight"] = 1
    elif species["is_legendary"]:
        data["weight"] = 10
    
    return data

async def fetch_all_pokemons() -> Dict[int, Dict[str, Any]]:
    async with aiohttp.ClientSession() as session:
        resp = await session.get(f"{BASE_URL}/pokemon?limit=2000")
        all_data = await resp.json()
        names = [p["name"] for p in all_data["results"]]
    
    pokemons = {}
    semaphore = asyncio.Semaphore(CONCURRENT)
    
    async def bounded_fetch(name):
        async with semaphore:
            return await fetch_pokemon(session, name)
    
    async with aiohttp.ClientSession() as session:
        tasks = [bounded_fetch(name) for name in names]
        results = await asyncio.gather(*tasks, return_exceptions=True)
        
        for data in results:
            if not isinstance(data, Exception):
                pokemons[data["id"]] = data
    
    return pokemons

pokemons = asyncio.run(fetch_all_pokemons())
with file_path.open("w") as f:
    json.dump(pokemons, f, indent=2)

print(f"Fetched {len(pokemons)} Pokémon!")
TODO:

- Discord Views:
  -Drop (should support multiple pokemon)
  -Inventory
  -View Pokemon (show sprite, stats, etc)

- Database:
  -Pokemon (pokemon_id, api_pokemon_id, encounter_event_id(FK), created_at, caught_by, owned_by, is_shiny, gender, caught_with, pokemon_stats_id (FK))

  -PokemonStats (stats_id, iv_hp, iv_atk, iv_def, iv_sp_atk, iv_sp_def, iv_speed, size)

  -EncounterEvent (encounter_id, created_at, triggered_by, pokemons_encountered_id)

  -Item (item_id, name, drop_chance, tradeable, )

  -ItemAttributes (attribute_id, item_id(FK), )

  -PlayerInventory (inventory_id, player_id)

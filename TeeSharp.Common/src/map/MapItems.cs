namespace TeeSharp.Common
{
    public enum MapItems
    {
        // game layer tiles
        ENTITY_NULL = 0,
        ENTITY_SPAWN,
        ENTITY_SPAWN_RED,
        ENTITY_SPAWN_BLUE,
        ENTITY_FLAGSTAND_RED,
        ENTITY_FLAGSTAND_BLUE,
        ENTITY_ARMOR_1,
        ENTITY_HEALTH_1,
        ENTITY_WEAPON_SHOTGUN,
        ENTITY_WEAPON_GRENADE,
        ENTITY_POWERUP_NINJA,
        ENTITY_WEAPON_RIFLE,
        NUM_ENTITIES,

        // GAME TILES
        TILE_AIR = 0,
        TILE_SOLID,
        TILE_DEATH,
        TILE_NOHOOK,

        ENTITY_COUNT = 255,
        ENTITY_OFFSET = 255 - 16 * 4,
    }
}
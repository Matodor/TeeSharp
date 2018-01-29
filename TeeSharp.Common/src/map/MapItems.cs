namespace TeeSharp.Common
{
    public enum MapItems
    {
        // GAME TILES
        TILE_AIR = 0,
        TILE_SOLID = 1 << 0 ,
        TILE_DEATH = 1 << 1,
        TILE_NOHOOK = 1 << 2,

        // GAME ENTITIES
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

        ENTITY_COUNT = 255,
        ENTITY_OFFSET = 255 - 16 * 4,
    }
}
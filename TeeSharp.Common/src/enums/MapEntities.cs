namespace TeeSharp.Common.Enums
{
    public enum MapEntities
    {
        Null = 0,
        Spawn,
        SpawnRed,
        SpawnBlue,
        FlagStandRed,
        FlagStandBlue,
        Armor,
        Health,
        WeaponShotgun,
        WeaponGrenade,
        PowerupNinja,
        WeaponLaser,

        NumEntities,
        EntityOffset = 255 - 16 * 4,
    }
}
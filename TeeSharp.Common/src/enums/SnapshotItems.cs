namespace TeeSharp.Common.Enums
{
    public enum SnapshotItems
    {
        Invalid = 0,

        PlayerInput,
        Projectile,
        Laser,
        Pickup,
        Flag,
        GameData,
        GameDataTeam,
        GameDataFlag,
        CharacterCore, // not used
        Character,
        PlayerInfo,
        SpectatorInfo,

        DemoClientInfo,
        DemoGameInfo,
        DemoTuneParams,

        EventCommon,
        EventExplosion,
        EventSpawn,
        EventHammerHit,
        EventDeath,
        EventSoundWorld,
        EventDamage,

        NumItems,
    }
}
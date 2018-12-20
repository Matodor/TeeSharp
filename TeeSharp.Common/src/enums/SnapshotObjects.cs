namespace TeeSharp.Common.Enums
{
    public enum SnapshotObjects
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

        NumObjects,
    }
}
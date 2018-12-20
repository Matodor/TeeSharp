namespace TeeSharp.Common.Enums
{
    public enum Weapon
    {
        Game = -3,  // team switching etc
        Self = -2,  // console kill command
        World = -1, // death tiles etc

        Hammer = 0,
        Gun,
        Shotgun,
        Grenade,
        Laser,
        Ninja,

        NumWeapons,
    }
}
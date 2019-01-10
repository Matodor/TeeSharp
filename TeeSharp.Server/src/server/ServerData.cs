using System;
using TeeSharp.Common.Enums;

namespace TeeSharp.Server
{
    public static class ServerData
    {
        public static readonly DataPickupsContainer Pickups = new DataPickupsContainer();
        public static readonly DataWeaponsContainer Weapons = new DataWeaponsContainer();
        public static readonly DataExplosion Explosion = new DataExplosion();
    }

    public class DataExplosion
    {
        public float Radius { get; set; } = 135f;
        public float MaxForce { get; set; } = 12f;
    }

    public class DataPickupsContainer
    {
        public DataPickupInfo this[Pickup pickup]
        {
            get
            {
                switch (pickup)
                {
                    case Pickup.Health:
                        return Data[0];
                    case Pickup.Armor:
                        return Data[1];
                    case Pickup.Grenade:
                        return Data[2];
                    case Pickup.Shotgun:
                        return Data[3];
                    case Pickup.Laser:
                        return Data[4];
                    case Pickup.Ninja:
                        return Data[5];
                    default:
                        throw new ArgumentOutOfRangeException(nameof(pickup), pickup, null);
                }
            }
        }

        public readonly DataPickupInfo[] Data = 
        {
            new DataPickupInfo {Name = "health", RespawnTime = 15, SpawnDelay = 0},
            new DataPickupInfo {Name = "armor", RespawnTime = 15, SpawnDelay = 0},
            new DataPickupInfo {Name = "grenade", RespawnTime = 15, SpawnDelay = 0},
            new DataPickupInfo {Name = "shotgun", RespawnTime = 15, SpawnDelay = 0},
            new DataPickupInfo {Name = "laser", RespawnTime = 15, SpawnDelay = 0},
            new DataPickupInfo {Name = "ninja", RespawnTime = 90, SpawnDelay = 90},
        };
    }

    public class DataWeaponsContainer
    {
        public DataWeaponInfo this[Weapon weapon]
        {
            get
            {
                switch (weapon)
                {
                    case Weapon.Hammer:
                        return Info[0];
                    case Weapon.Gun:
                        return Info[1];
                    case Weapon.Shotgun:
                        return Info[2];
                    case Weapon.Grenade:
                        return Info[3];
                    case Weapon.Laser:
                        return Info[4];
                    case Weapon.Ninja:
                        return Info[5];
                    default:
                        throw new ArgumentOutOfRangeException(nameof(weapon), weapon, null);
                }
            }
        }

        public DataWeaponInfoHammer Hammer => (DataWeaponInfoHammer) Info[0];
        public DataWeaponInfoGun Gun => (DataWeaponInfoGun) Info[1];
        public DataWeaponInfoShotgun Shotgun => (DataWeaponInfoShotgun) Info[2];
        public DataWeaponInfoGrenade Grenade => (DataWeaponInfoGrenade) Info[3];
        public DataWeaponInfoRifle Laser => (DataWeaponInfoRifle) Info[4];
        public DataWeaponInfoNinja Ninja => (DataWeaponInfoNinja) Info[5];

        public readonly DataWeaponInfo[] Info = 
        {
            new DataWeaponInfoHammer
            {
                Name = "hammer",
                FireDelay = 125,
                MaxAmmo = 10,
                AmmoRegenTime = 0,
                Damage = 3,
            },
            new DataWeaponInfoGun
            {
                Name = "gun",
                FireDelay = 125,
                MaxAmmo = 10,
                AmmoRegenTime = 500,
                Damage = 1,
                Curvature = 1.25f,
                Speed = 2200.0f,
                Lifetime = 2.00f
            },
            new DataWeaponInfoShotgun
            {
                Name = "shotgun",
                FireDelay = 500,
                MaxAmmo = 10,
                AmmoRegenTime = 0,
                Damage = 1,
                Curvature = 1.25f,
                Speed = 2200f,
                SpeedDiff = 0.80f,
                Lifetime = 0.250f,
            },
            new DataWeaponInfoGrenade
            {
                Name = "grenade",
                FireDelay = 500,
                MaxAmmo = 10,
                AmmoRegenTime = 0,
                Damage = 6,
                Curvature = 7f,
                Speed = 1000f,
                Lifetime = 2f,
            },
            new DataWeaponInfoRifle
            {
                Name = "rifle",
                FireDelay = 800,
                MaxAmmo = 10,
                AmmoRegenTime = 0,
                Damage = 5,
                Reach = 800f,
                BounceDelay = 150,
                BounceNum = 1,
                BounceCost = 0f
            },
            new DataWeaponInfoNinja
            {
                Name = "ninja",
                FireDelay = 800,
                MaxAmmo = 10,
                AmmoRegenTime = 0,
                Damage = 9,
                Duration = 15000,
                MoveTime = 200,
                Velocity = 50
            },
        };
    }

    public class DataWeaponInfoHammer : DataWeaponInfo
    {
        
    }

    public class DataWeaponInfoGun : DataWeaponInfo
    {
        public float Curvature { get; set; }
        public float Speed { get; set; }
        public float Lifetime { get; set; }
    }

    public class DataWeaponInfoShotgun : DataWeaponInfo
    {
        public float Curvature { get; set; }
        public float Speed { get; set; }
        public float SpeedDiff { get; set; }
        public float Lifetime { get; set; }
    }

    public class DataWeaponInfoGrenade : DataWeaponInfo
    {
        public float Curvature { get; set; }
        public float Speed { get; set; }
        public float Lifetime { get; set; }
    }

    public class DataWeaponInfoRifle : DataWeaponInfo
    {
        public float Reach { get; set; }
        public int BounceDelay { get; set; }
        public int BounceNum { get; set; }
        public float BounceCost { get; set; }
    }

    public class DataWeaponInfoNinja : DataWeaponInfo
    {
        public int Duration { get; set; }
        public int MoveTime { get; set; }
        public int Velocity { get; set; }
    }

    public class DataWeaponInfo
    {
        public string Name { get; set; }
        public int FireDelay { get; set; }
        public int MaxAmmo { get; set; }
        public int AmmoRegenTime { get; set; }
        public int Damage { get; set; }
    }

    public class DataPickupInfo
    {
        public string Name { get; set; }
        public int RespawnTime { get; set; }
        public int SpawnDelay { get; set; }
    }
}
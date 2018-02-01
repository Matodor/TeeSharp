namespace TeeSharp.Server
{
    public static class ServerData
    {
        public static readonly DataContainer Data = new DataContainer();
    }

    public class DataContainer
    {
        public readonly DataPickupInfo[] Pickups = new []
        {
            new DataPickupInfo {Name = "health", RespawnTime = 15, Spawndelay = 0},
            new DataPickupInfo {Name = "armor", RespawnTime = 15, Spawndelay = 0},
            new DataPickupInfo {Name = "weapon", RespawnTime = 15, Spawndelay = 0},
            new DataPickupInfo {Name = "ninja", RespawnTime = 90, Spawndelay = 90},
        };

        public readonly DataWeaponsContainer Weapons = new DataWeaponsContainer();
    }

    public class DataWeaponsContainer
    {
        public DataWeaponInfoHammer Hammer => (DataWeaponInfoHammer) Info[0];
        public DataWeaponInfoGun Gun => (DataWeaponInfoGun) Info[0];
        public DataWeaponInfoShotgun Shotgun => (DataWeaponInfoShotgun) Info[0];
        public DataWeaponInfoGrenade Grenade => (DataWeaponInfoGrenade) Info[0];
        public DataWeaponInfoRifle Rifle => (DataWeaponInfoRifle) Info[0];
        public DataWeaponInfoNinja Ninja => (DataWeaponInfoNinja) Info[0];

        public readonly DataWeaponInfo[] Info = new DataWeaponInfo[]
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
                Damage = 1,
                Curvature = 7f,
                Speed = 1000f,
                Lifetime = 2f,
            },
            new DataWeaponInfoRifle
            {
                Name = "rifle",
                FireDelay = 500,
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
                FireDelay = 500,
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
        public float Curvature;
        public float Speed;
        public float Lifetime;
    }

    public class DataWeaponInfoShotgun : DataWeaponInfo
    {
        public float Curvature;
        public float Speed;
        public float SpeedDiff;
        public float Lifetime;
    }

    public class DataWeaponInfoGrenade : DataWeaponInfo
    {
        public float Curvature;
        public float Speed;
        public float Lifetime;
    }

    public class DataWeaponInfoRifle : DataWeaponInfo
    {
        public float Reach;
        public int BounceDelay;
        public int BounceNum;
        public float BounceCost;
    }

    public class DataWeaponInfoNinja : DataWeaponInfo
    {
        public int Duration;
        public int MoveTime;
        public int Velocity;
    }


    public class DataWeaponInfo
    {
        public string Name;
        public int FireDelay;
        public int MaxAmmo;
        public int AmmoRegenTime;
        public int Damage;
    }

    public class DataPickupInfo
    {
        public string Name { get; set; }
        public int RespawnTime { get; set; }
        public int Spawndelay { get; set; }
    }
}
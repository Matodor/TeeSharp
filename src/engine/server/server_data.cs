using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teecsharp
{
    public class CDataContainer
    {
        static CDataPickupspec[] x19 = new CDataPickupspec[]
        {
            /* x19[0] */ new CDataPickupspec { m_pName = "health", m_Respawntime = 15, m_Spawndelay = 0 },
            /* x19[1] */ new CDataPickupspec { m_pName = "armor", m_Respawntime = 15, m_Spawndelay = 0 },
            /* x19[2] */ new CDataPickupspec { m_pName = "weapon", m_Respawntime = 15, m_Spawndelay = 0 },
            /* x19[3] */ new CDataPickupspec { m_pName = "ninja", m_Respawntime = 90, m_Spawndelay = 90 },
        };

        static CDataWeaponspec[] x1887 = new CDataWeaponspec[]
        {
            new CDataWeaponspec { m_pName = "hammer",   m_Firedelay = 125, m_Maxammo = 10, m_Ammoregentime = 0,     m_Damage = 3},
            new CDataWeaponspec { m_pName = "gun",      m_Firedelay = 125, m_Maxammo = 10, m_Ammoregentime = 500,   m_Damage = 1},
            new CDataWeaponspec { m_pName = "shotgun",  m_Firedelay = 500, m_Maxammo = 10, m_Ammoregentime = 0,     m_Damage = 1},
            new CDataWeaponspec { m_pName = "grenade",  m_Firedelay = 500, m_Maxammo = 10, m_Ammoregentime = 0,     m_Damage = 1},
            new CDataWeaponspec { m_pName = "rifle",    m_Firedelay = 500, m_Maxammo = 10, m_Ammoregentime = 0,     m_Damage = 5},
            new CDataWeaponspec { m_pName = "ninja",    m_Firedelay = 500, m_Maxammo = 10, m_Ammoregentime = 0,     m_Damage = 9},
        };

        public static CDataContainer datacontainer = new CDataContainer
        {
            m_NumPickups = 4,
            m_aPickups = x19,
            m_Weapons = new CDataWeaponspecs
            {
                m_Hammer = new CDataWeaponspecHammer { m_pBase = x1887[0] },
                m_Gun = new CDataWeaponspecGun { m_pBase = x1887[1], m_Curvature = 1.250f, m_Speed = 2200.0f, m_Lifetime = 2.00f },
                m_Shotgun = new CDataWeaponspecShotgun { m_pBase = x1887[2], m_Curvature = 1.250f, m_Speed = 2200.0f, m_Speeddiff = 0.80f, m_Lifetime = 0.250f },
                m_Grenade = new CDataWeaponspecGrenade { m_pBase = x1887[3], m_Curvature = 7.000f, m_Speed = 1000.0f, m_Lifetime = 2.0f },
                m_Rifle = new CDataWeaponspecRifle { m_pBase = x1887[4], m_Reach = 800.0f, m_BounceDelay = 150, m_BounceNum = 1, m_BounceCost = 0.0f },
                m_Ninja = new CDataWeaponspecNinja { m_pBase = x1887[5], m_Duration = 15000, m_Movetime = 200, m_Velocity = 50 },
                m_aId = x1887,
                m_NumId = 6,
            }
        };

        public int m_NumPickups;
        public CDataPickupspec[] m_aPickups;
        public CDataWeaponspecs m_Weapons;
    }

    public class CDataWeaponspecs
    {
        public CDataWeaponspecHammer m_Hammer;
        public CDataWeaponspecGun m_Gun;
        public CDataWeaponspecShotgun m_Shotgun;
        public CDataWeaponspecGrenade m_Grenade;
        public CDataWeaponspecRifle m_Rifle;
        public CDataWeaponspecNinja m_Ninja;
        public int m_NumId;
        public CDataWeaponspec[] m_aId;
    }

    public struct CDataWeaponspecHammer
    {
        public CDataWeaponspec m_pBase;
    }

    public struct CDataWeaponspecGun
    {
        public CDataWeaponspec m_pBase;
        public float m_Curvature;
        public float m_Speed;
        public float m_Lifetime;
    }

    public struct CDataWeaponspecShotgun
    {
        public CDataWeaponspec m_pBase;
        public float m_Curvature;
        public float m_Speed;
        public float m_Speeddiff;
        public float m_Lifetime;
    }

    public struct CDataWeaponspecGrenade
    {
        public CDataWeaponspec m_pBase;
        public float m_Curvature;
        public float m_Speed;
        public float m_Lifetime;
    }

    public struct CDataWeaponspecRifle
    {
        public CDataWeaponspec m_pBase;
        public float m_Reach;
        public int m_BounceDelay;
        public int m_BounceNum;
        public float m_BounceCost;
    }

    public struct CDataWeaponspecNinja
    {
        public CDataWeaponspec m_pBase;
        public int m_Duration;
        public int m_Movetime;
        public int m_Velocity;
    }

    public class CDataWeaponspec
    {
        public string m_pName;
        public int m_Firedelay;
        public int m_Maxammo;
        public int m_Ammoregentime;
        public int m_Damage;
    }

    public class CDataPickupspec
    {
        public string m_pName;
        public int m_Respawntime;
        public int m_Spawndelay;
    }
}

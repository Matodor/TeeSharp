using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Teecsharp
{
    class CPickup : CEntity
    {
        private readonly CDataContainer g_pData = CDataContainer.datacontainer;
        const int PickupPhysSize = 14;

        private readonly int m_Type;
        private readonly int m_Subtype;
        private int m_SpawnTick;

        public CPickup(CGameWorld pGameWorld, int Type, int SubType) : base(pGameWorld, CGameWorld.ENTTYPE_PICKUP)
        {
            m_Type = Type;
            m_Subtype = SubType;
            m_ProximityRadius = PickupPhysSize;

            Reset();

            GameWorld.InsertEntity(this);
        }

        public override void Reset()
        {
            if (g_pData.m_aPickups[m_Type].m_Spawndelay > 0)
                m_SpawnTick = Server.Tick() + Server.TickSpeed() * g_pData.m_aPickups[m_Type].m_Spawndelay;
            else
                m_SpawnTick = -1;
        }

        public override void Tick()
        {
            // wait for respawn
            if (m_SpawnTick > 0)
            {
                if (Server.Tick() > m_SpawnTick)
                {
                    // respawn
                    m_SpawnTick = -1;

                    if (m_Type == (int)Consts.POWERUP_WEAPON)
                        GameServer.CreateSound(m_Pos, (int)Consts.SOUND_WEAPON_SPAWN);
                }
                else
                    return;
            }
            // Check if a player intersected us
            CCharacter pChr = GameServer.m_World.ClosestCharacter(m_Pos, 20.0f, null);
            if (pChr != null && pChr.IsAlive())
            {
                // player picked us up, is someone was hooking us, let them go
                int RespawnTime = -1;
                switch (m_Type)
                {
                    case (int)Consts.POWERUP_HEALTH:
                    if (pChr.IncreaseHealth(1))
                    {
                        GameServer.CreateSound(m_Pos, (int)Consts.SOUND_PICKUP_HEALTH);
                        RespawnTime = g_pData.m_aPickups[m_Type].m_Respawntime;
                    }
                    break;

                    case (int)Consts.POWERUP_ARMOR:
                    if (pChr.IncreaseArmor(1))
                    {
                        GameServer.CreateSound(m_Pos, (int)Consts.SOUND_PICKUP_ARMOR);
                        RespawnTime = g_pData.m_aPickups[m_Type].m_Respawntime;
                    }
                    break;

                    case (int)Consts.POWERUP_WEAPON:
                    if (m_Subtype >= 0 && m_Subtype < (int)Consts.NUM_WEAPONS)
                    {
                        if (pChr.GiveWeapon(m_Subtype, 10))
                        {
                            RespawnTime = g_pData.m_aPickups[m_Type].m_Respawntime;

                            if (m_Subtype == (int)Consts.WEAPON_GRENADE)
                                GameServer.CreateSound(m_Pos, (int)Consts.SOUND_PICKUP_GRENADE);
                            else if (m_Subtype == (int)Consts.WEAPON_SHOTGUN)
                                GameServer.CreateSound(m_Pos, (int)Consts.SOUND_PICKUP_SHOTGUN);
                            else if (m_Subtype == (int)Consts.WEAPON_RIFLE)
                                GameServer.CreateSound(m_Pos, (int)Consts.SOUND_PICKUP_SHOTGUN);

                            if (pChr.GetPlayer() != null)
                                GameServer.SendWeaponPickup(pChr.GetPlayer().GetCID(), m_Subtype);
                        }
                    }
                    break;

                    case (int)Consts.POWERUP_NINJA:
                    {
                        // activate ninja on target player
                        pChr.GiveNinja();
                        RespawnTime = g_pData.m_aPickups[m_Type].m_Respawntime;

                        // loop through all players, setting their emotes
                        CCharacter pC = (CCharacter)GameServer.m_World.FindFirst(CGameWorld.ENTTYPE_CHARACTER);
                        for (; pC != null; pC = (CCharacter)pC.TypeNext())
                        {
                            if (pC != pChr)
                                pC.SetEmote((int)Consts.EMOTE_SURPRISE, Server.Tick() + Server.TickSpeed());
                        }

                        pChr.SetEmote((int)Consts.EMOTE_ANGRY, Server.Tick() + 1200 * Server.TickSpeed() / 1000);
                        break;
                    }
                };

                if (RespawnTime >= 0)
                {
                    string aBuf = string.Format("pickup player='{0}:{1}' item={2}/{3}",
                        pChr.GetPlayer().GetCID(), Server.ClientName(pChr.GetPlayer().GetCID()), m_Type, m_Subtype);
                    GameServer.Console.Print(IConsole.OUTPUT_LEVEL_DEBUG, "game", aBuf);
                    m_SpawnTick = Server.Tick() + Server.TickSpeed() * RespawnTime;
                }
            }
        }

        public override void TickPaused()
        {
            if (m_SpawnTick != -1)
                ++m_SpawnTick;
        }

        public override void Snap(int SnappingClient)
        {
            if (m_SpawnTick != -1 || NetworkClipped(SnappingClient))
                return;

            CNetObj_Pickup pP = Server.SnapNetObj<CNetObj_Pickup>((int)Consts.NETOBJTYPE_PICKUP, m_IDs[0]);

            if (pP == null)
                return;

            pP.m_X = (int)m_Pos.x;
            pP.m_Y = (int)m_Pos.y;
            pP.m_Type = m_Type;
            pP.m_Subtype = m_Subtype;
        }
    }
}

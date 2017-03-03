using System;

namespace Teecsharp
{
    public class CGameControllerTDM : IGameController
    {
        public CGameControllerTDM(CGameContext pGameServer) : base(pGameServer)
        {
            m_pGameType = "TDM";
            m_GameFlags = (int)Consts.GAMEFLAG_TEAMS;
        }

        public override int OnCharacterDeath(CCharacter pVictim, CPlayer pKiller, int Weapon)
        {
            base.OnCharacterDeath(pVictim, pKiller, Weapon);

            if (pKiller != null && Weapon != CCharacter.WEAPON_GAME)
            {
                // do team scoring
                if (pKiller == pVictim.GetPlayer() || pKiller.GetTeam() == pVictim.GetPlayer().GetTeam())
                    m_aTeamscore[pKiller.GetTeam() & 1]--; // klant arschel
                else
                    m_aTeamscore[pKiller.GetTeam() & 1]++; // good shit
            }

            pVictim.GetPlayer().m_RespawnTick = Math.Max(pVictim.GetPlayer().m_RespawnTick, Server.Tick() + Server.TickSpeed() * g_Config.GetInt("SvRespawnDelayTDM"));

            return 0;
        }

        public override void Snap(int SnappingClient)
        {
            base.Snap(SnappingClient);

            CNetObj_GameData pGameDataObj = Server.SnapNetObj<CNetObj_GameData>((int)Consts.NETOBJTYPE_GAMEDATA, 0);
            if (pGameDataObj == null)
                return;

            pGameDataObj.m_TeamscoreRed = m_aTeamscore[(int)Consts.TEAM_RED];
            pGameDataObj.m_TeamscoreBlue = m_aTeamscore[(int)Consts.TEAM_BLUE];

            pGameDataObj.m_FlagCarrierRed = 0;
            pGameDataObj.m_FlagCarrierBlue = 0;
        }

        public override void Tick()
        {
            base.Tick();
        }
    }
}

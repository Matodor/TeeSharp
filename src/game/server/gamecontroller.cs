using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using vec2 = Teecsharp.vector2_float;

namespace Teecsharp
{
    /*
        Class: Game Controller
            Controls the main game logic. Keeping track of team and player score,
            winning conditions and specific game logic.
    */

    public class CSpawnEval
    {
        public vec2 m_Pos;
        public bool m_Got;
        public int m_FriendlyTeam;
        public float m_Score;

        public CSpawnEval()
        {
            m_Got = false;
            m_FriendlyTeam = -1;
            m_Pos = new vec2(100, 100);
        }
    }

    public abstract class IGameController
    {
        public bool m_BlockTeam;
        public int m_TimeMusic;

        public int m_aPrevTeam;
        public int[] m_aTeamscore;
        public int m_TimeToResetGame;
        public bool m_EndRound;
        public string m_pGameType;

        protected CGameContext GameServer
        {
            get { return m_pGameServer; }
        }

        protected IServer Server
        {
            get { return m_pServer; }
        }

        protected string m_aMapWish;

        protected int m_RoundStartTick;
        protected int m_GameOverTick;
        protected int m_SuddenDeath;

        protected int m_Warmup;
        protected int m_RoundCount;

        protected int m_GameFlags;
        protected int m_UnbalancedTick;
        protected bool m_ForceBalanced;

        protected CConfiguration g_Config;
        private readonly vec2[,] m_aaSpawnPoints;
        private readonly int[] m_aNumSpawnPoints;

        private readonly CGameContext m_pGameServer;
        private readonly IServer m_pServer;

        public IGameController(CGameContext pGameServer)
        {
            m_pGameServer = pGameServer;
            m_pServer = m_pGameServer.Server;
            m_pGameType = "unknown";
            g_Config = CConfiguration.Instance;

            //
            DoWarmup(g_Config.GetInt("SvWarmup"));
            m_aaSpawnPoints = new vec2[3, 64];
            m_aTeamscore = new int[2];
            m_aNumSpawnPoints = new int[3];
            m_GameOverTick = -1;
            m_SuddenDeath = 0;
            m_RoundStartTick = Server.Tick();
            m_RoundCount = 0;
            m_GameFlags = 0;
            m_aTeamscore[(int)Consts.TEAM_RED] = 0;
            m_aTeamscore[(int)Consts.TEAM_BLUE] = 0;
            m_aMapWish = null;

            m_UnbalancedTick = -1;
            m_ForceBalanced = false;

            m_aNumSpawnPoints[0] = 0;
            m_aNumSpawnPoints[1] = 0;
            m_aNumSpawnPoints[2] = 0;
        }

        ~IGameController()
        {

        }

        protected float EvaluateSpawnPos(CSpawnEval pEval, vec2 Pos)
        {
            float Score = 0.0f;
            CCharacter pC = (CCharacter)GameServer.m_World.FindFirst(CGameWorld.ENTTYPE_CHARACTER);
            for (; pC != null; pC = (CCharacter)pC.TypeNext())
            {
                // team mates are not as dangerous as enemies
                float Scoremod = 1.0f;
                if (pEval.m_FriendlyTeam != -1 && pC.GetPlayer().GetTeam() == pEval.m_FriendlyTeam)
                    Scoremod = 0.5f;

                float d = VMath.distance(Pos, pC.m_Pos);
                Score += Scoremod * (d == 0 ? 1000000000.0f : 1.0f / d);
            }

            return Score;
        }

        protected void EvaluateSpawnType(CSpawnEval pEval, int Type)
        {
            // get spawn point
            for (int i = 0; i < m_aNumSpawnPoints[Type]; i++)
            {
                // check if the position is occupado
                CEntity[] aEnts = new CEntity[(int)Consts.MAX_CLIENTS];
                int Num = GameServer.m_World.FindEntities(m_aaSpawnPoints[Type, i], 64, ref aEnts, (int)Consts.MAX_CLIENTS, CGameWorld.ENTTYPE_CHARACTER);
                vec2[] Positions = new vec2[5] { new vec2(0.0f, 0.0f), new vec2(-32.0f, 0.0f), new vec2(0.0f, -32.0f), new vec2(32.0f, 0.0f), new vec2(0.0f, 32.0f) }; // start, left, up, right, down
                int Result = -1;
                for (int Index = 0; Index < 5 && Result == -1; ++Index)
                {
                    Result = Index;
                    for (int c = 0; c < Num; ++c)
                        if (GameServer.Collision.CheckPoint(m_aaSpawnPoints[Type, i] + Positions[Index]) ||
                            VMath.distance(aEnts[c].m_Pos, m_aaSpawnPoints[Type, i] + Positions[Index]) <= aEnts[c].m_ProximityRadius)
                        {
                            Result = -1;
                            break;
                        }
                }
                if (Result == -1)
                    continue;   // try next spawn point

                vec2 P = m_aaSpawnPoints[Type, i] + Positions[Result];
                float S = EvaluateSpawnPos(pEval, P);
                if (!pEval.m_Got || pEval.m_Score > S)
                {
                    pEval.m_Got = true;
                    pEval.m_Score = S;
                    pEval.m_Pos = P;
                }
            }
        }

        //
        public virtual bool CanSpawn(int Team, ref vec2 pOutPos)
        {
            CSpawnEval Eval = new CSpawnEval();

            // spectators can't spawn
            if (Team == (int)Consts.TEAM_SPECTATORS)
                return false;

            if (IsTeamplay())
            {
                Eval.m_FriendlyTeam = Team;

                // first try own team spawn, then normal spawn and then enemy
                EvaluateSpawnType(Eval, 1 + (Team & 1));
                if (!Eval.m_Got)
                {
                    EvaluateSpawnType(Eval, 0);
                    if (!Eval.m_Got)
                        EvaluateSpawnType(Eval, 1 + ((Team + 1) & 1));
                }
            }
            else
            {
                EvaluateSpawnType(Eval, 0);
                EvaluateSpawnType(Eval, 1);
                EvaluateSpawnType(Eval, 2);
            }

            pOutPos = Eval.m_Pos;
            return Eval.m_Got;
        }

        protected void CycleMap()
        {
            if (!string.IsNullOrEmpty(m_aMapWish))
            {
                string aBuf = string.Format("rotating map to {0}", m_aMapWish);
                GameServer.Console.Print(IConsole.OUTPUT_LEVEL_DEBUG, "game", aBuf);
                g_Config.SetString("SvMap", m_aMapWish);
                m_aMapWish = null;
                m_RoundCount = 0;
                return;
            }
            if (string.IsNullOrEmpty(g_Config.GetString("SvMaprotation")))
                return;

            if (m_RoundCount < g_Config.GetInt("SvRoundsPerMap") - 1)
            {
                if (g_Config.GetInt("SvRoundSwap") != 0)
                    GameServer.SwapTeams();
                return;
            }

            // handle maprotation
            string pMapRotation = g_Config.GetString("SvMaprotation");
            string pCurrentMap = g_Config.GetString("SvMap");

            /*int CurrentMapLen = pCurrentMap.Length;
            string pNextMap = pMapRotation;
            while (!string.IsNullOrEmpty(pNextMap))
            {
                int WordLen = 0;
                while (pNextMap[WordLen] != '\0' && !IsSeparator(pNextMap[WordLen]))
                    WordLen++;

                if (WordLen == CurrentMapLen && CSystem.str_comp_num(pNextMap, pCurrentMap, CurrentMapLen))
                {
                    // map found
                    pNextMap += CurrentMapLen;
                    while (*pNextMap && IsSeparator(*pNextMap))
                        pNextMap++;

                    break;
                }

                pNextMap++;
            }

            // restart rotation
            if (pNextMap[0] == 0)
                pNextMap = pMapRotation;

            // cut out the next map
            char aBuf[512];
            for (int i = 0; i < 512; i++)
            {
                aBuf[i] = pNextMap[i];
                if (IsSeparator(pNextMap[i]) || pNextMap[i] == 0)
                {
                    aBuf[i] = 0;
                    break;
                }
            }

            // skip spaces
            int i = 0;
            while (IsSeparator(aBuf[i]))
                i++;

            m_RoundCount = 0;

            char aBufMsg[256];
            str_format(aBufMsg, sizeof(aBufMsg), "rotating map to %s", &aBuf[i]);
            GameServer.Console.Print(IConsole::OUTPUT_LEVEL_DEBUG, "game", aBuf);
            str_copy(g_Config.["SvMap, &aBuf[i], sizeof(g_Config.["SvMap));*/
        }

        protected void ResetGame()
        {
            GameServer.m_World.m_ResetRequested = true;
        }

        public bool IsTeamplay()
        {
            return (m_GameFlags & (int)Consts.GAMEFLAG_TEAMS) != 0;
        }

        public bool CheckTeamBalance()
        {
            if (!IsTeamplay() || g_Config.GetInt("SvTeambalanceTime") == 0)
                return true;

            int[] aT = new int[2] { 0, 0 };
            for (int i = 0; i < (int)Consts.MAX_CLIENTS; i++)
            {
                CPlayer pP = GameServer.m_apPlayers[i];
                if (pP != null && pP.GetTeam() != (int)Consts.TEAM_SPECTATORS)
                    aT[pP.GetTeam()]++;
            }

            string aBuf = "";
            if (Math.Abs(aT[0] - aT[1]) >= 2)
            {
                aBuf = string.Format("Teams are NOT balanced (red={0} blue={1})", aT[0], aT[1]);
                GameServer.Console.Print(IConsole.OUTPUT_LEVEL_DEBUG, "game", aBuf);
                if (GameServer.Controller.m_UnbalancedTick == -1)
                    GameServer.Controller.m_UnbalancedTick = Server.Tick();
                return false;
            }
            else
            {
                aBuf = string.Format("Teams are balanced (red={0} blue={1})", aT[0], aT[1]);
                GameServer.Console.Print(IConsole.OUTPUT_LEVEL_DEBUG, "game", aBuf);
                GameServer.Controller.m_UnbalancedTick = -1;
                return true;
            }
        }

        public bool CanChangeTeam(CPlayer pPlayer, int JoinTeam)
        {
            int[] aT = new int[2] { 0, 0 };

            if (!IsTeamplay() || JoinTeam == (int)Consts.TEAM_SPECTATORS || g_Config.GetInt("SvTeambalanceTime") == 0)
                return true;

            for (int i = 0; i < (int)Consts.MAX_CLIENTS; i++)
            {
                CPlayer pP = GameServer.m_apPlayers[i];
                if (pP != null && pP.GetTeam() != (int)Consts.TEAM_SPECTATORS)
                    aT[pP.GetTeam()]++;
            }

            // simulate what would happen if changed team
            aT[JoinTeam]++;
            if (pPlayer.GetTeam() != (int)Consts.TEAM_SPECTATORS)
                aT[JoinTeam ^ 1]--;

            // there is a player-difference of at least 2
            if (Math.Abs(aT[0] - aT[1]) >= 2)
            {
                // player wants to join team with less players
                if ((aT[0] < aT[1] && JoinTeam == (int)Consts.TEAM_RED) || (aT[0] > aT[1] && JoinTeam == (int)Consts.TEAM_BLUE))
                    return true;
                return false;
            }
            return true;
        }

        public int ClampTeam(int Team)
        {
            if (Team < 0)
                return (int)Consts.TEAM_SPECTATORS;
            if (IsTeamplay())
                return Team & 1;
            return 0;
        }

        public void DoWarmup(int Seconds)
        {
            if (Seconds < 0)
                m_Warmup = 0;
            else
                m_Warmup = Seconds * Server.TickSpeed();
        }

        public void StartRound()
        {
            ResetGame();

            m_RoundStartTick = Server.Tick();
            m_SuddenDeath = 0;
            m_GameOverTick = -1;
            GameServer.m_World.m_Paused = false;
            m_aTeamscore[(int)Consts.TEAM_RED] = 0;
            m_aTeamscore[(int)Consts.TEAM_BLUE] = 0;
            m_ForceBalanced = false;
            string aBuf = string.Format("start round type='{0}' teamplay='{1}'", m_pGameType, m_GameFlags & (int)Consts.GAMEFLAG_TEAMS);
            GameServer.Console.Print(IConsole.OUTPUT_LEVEL_DEBUG, "game", aBuf);
        }

        public void EndRound()
        {
            if (m_Warmup != 0) // game can't end when we are running warmup
                return;

            GameServer.m_World.m_Paused = true;
            m_GameOverTick = Server.Tick();
            m_SuddenDeath = 0;
        }

        public void ChangeMap(string pToMap)
        {
            m_aMapWish = pToMap;
            EndRound();
        }

        public bool IsFriendlyFire(int ClientID1, int ClientID2)
        {
            if (ClientID1 == ClientID2)
                return false;

            if (IsTeamplay())
            {
                if (GameServer.m_apPlayers[ClientID1] == null || GameServer.m_apPlayers[ClientID2] == null)
                    return false;

                if (GameServer.m_apPlayers[ClientID1].GetTeam() == GameServer.m_apPlayers[ClientID2].GetTeam())
                    return true;
            }

            return false;
        }


        public bool IsForceBalanced()
        {
            if (m_ForceBalanced)
            {
                m_ForceBalanced = false;
                return true;
            }
            return false;
        }

        public virtual void DoWincheck()
        {
            if (m_GameOverTick == -1 && m_Warmup == 0 && !GameServer.m_World.m_ResetRequested)
            {
                int SvScorelimit = g_Config.GetInt("SvScorelimit");
                int SvTimelimit = g_Config.GetInt("SvTimelimit");
                if (IsTeamplay())
                {
                    // check score win condition
                    if ((SvScorelimit > 0 && (m_aTeamscore[(int)Consts.TEAM_RED] >= SvScorelimit || m_aTeamscore[(int)Consts.TEAM_BLUE] >= SvScorelimit)) ||
                        (SvTimelimit > 0 && (Server.Tick() - m_RoundStartTick) >= SvTimelimit * Server.TickSpeed() * 60))
                    {
                        if (m_aTeamscore[(int)Consts.TEAM_RED] != m_aTeamscore[(int)Consts.TEAM_BLUE])
                            EndRound();
                        else
                            m_SuddenDeath = 1;
                    }
                }
                else
                {
                    // gather some stats
                    int Topscore = 0;
                    int TopscoreCount = 0;
                    for (int i = 0; i < (int)Consts.MAX_CLIENTS; i++)
                    {
                        if (GameServer.m_apPlayers[i] != null)
                        {
                            if (GameServer.m_apPlayers[i].m_Score > Topscore)
                            {
                                Topscore = GameServer.m_apPlayers[i].m_Score;
                                TopscoreCount = 1;
                            }
                            else if (GameServer.m_apPlayers[i].m_Score == Topscore)
                                TopscoreCount++;
                        }
                    }

                    // check score win condition
                    if ((SvScorelimit > 0 && Topscore >= SvScorelimit) ||
                        (SvTimelimit > 0 && (Server.Tick() - m_RoundStartTick) >= SvTimelimit * Server.TickSpeed() * 60))
                    {
                        if (TopscoreCount == 1)
                            EndRound();
                        else
                            m_SuddenDeath = 1;
                    }
                }
            }
        }

        /*

        */

        public virtual bool CanBeMovedOnBalance(int ClientID)
        {
            return true;
        }

        public virtual void Tick()
        {
            // do warmup
            if (m_Warmup != 0)
            {
                m_Warmup--;
                if (m_Warmup == 0)
                    StartRound();
            }

            if (m_GameOverTick != -1)
            {
                // game over.. wait for restart
                if (Server.Tick() > m_GameOverTick + Server.TickSpeed() * 10)
                {
                    CycleMap();
                    StartRound();
                    m_RoundCount++;
                }
            }

            // game is Paused
            if (GameServer.m_World.m_Paused)
                ++m_RoundStartTick;

            // do team-balancing
            if (IsTeamplay() && m_UnbalancedTick != -1 && Server.Tick() > m_UnbalancedTick + g_Config.GetInt("SvTeambalanceTime") * Server.TickSpeed() * 60)
            {
                GameServer.Console.Print(IConsole.OUTPUT_LEVEL_DEBUG, "game", "Balancing teams");

                int[] aT = { 0, 0 };
                float[] aTScore = { 0, 0 };
                float[] aPScore = new float[(int)Consts.MAX_CLIENTS];

                for (int i = 0; i < (int)Consts.MAX_CLIENTS; i++)
                {
                    if (GameServer.m_apPlayers[i] != null && GameServer.m_apPlayers[i].GetTeam() != (int)Consts.TEAM_SPECTATORS)
                    {
                        aT[GameServer.m_apPlayers[i].GetTeam()]++;
                        aPScore[i] = GameServer.m_apPlayers[i].m_Score * Server.TickSpeed() * 60.0f /
                            (Server.Tick() - GameServer.m_apPlayers[i].m_ScoreStartTick);
                        aTScore[GameServer.m_apPlayers[i].GetTeam()] += aPScore[i];
                    }
                }

                // are teams unbalanced?
                if (Math.Abs(aT[0] - aT[1]) >= 2)
                {
                    int M = (aT[0] > aT[1]) ? 0 : 1;
                    int NumBalance = Math.Abs(aT[0] - aT[1]) / 2;

                    do
                    {
                        CPlayer pP = null;
                        float PD = aTScore[M];
                        for (int i = 0; i < (int)Consts.MAX_CLIENTS; i++)
                        {
                            if (GameServer.m_apPlayers[i] == null || !CanBeMovedOnBalance(i))
                                continue;
                            // remember the player who would cause lowest score-difference
                            if (GameServer.m_apPlayers[i].GetTeam() == M && (pP == null || Math.Abs((aTScore[M ^ 1] + aPScore[i]) - (aTScore[M] - aPScore[i])) < PD))
                            {
                                pP = GameServer.m_apPlayers[i];
                                PD = Math.Abs((aTScore[M ^ 1] + aPScore[i]) - (aTScore[M] - aPScore[i]));
                            }
                        }

                        // move the player to the other team
                        int Temp = pP.m_LastActionTick;
                        pP.SetTeam(M ^ 1);
                        pP.m_LastActionTick = Temp;

                        pP.Respawn();
                        pP.m_ForceBalanced = true;
                    } while (--NumBalance != 0);

                    m_ForceBalanced = true;
                }
                m_UnbalancedTick = -1;
            }

            // check for inactive players
            if (g_Config.GetInt("SvInactiveKickTime") > 0)
            {
                for (int i = 0; i < (int)Consts.MAX_CLIENTS; ++i)
                {
                    if (GameServer.m_apPlayers[i] != null && GameServer.m_apPlayers[i].GetTeam() != (int)Consts.TEAM_SPECTATORS && !Server.IsAuthed(i))
                    {
                        if (Server.Tick() > GameServer.m_apPlayers[i].m_LastActionTick + g_Config.GetInt("SvInactiveKickTime") * Server.TickSpeed() * 60)
                        {
                            switch (g_Config.GetInt("SvInactiveKick"))
                            {
                                case 0:
                                {
                                    // move player to spectator
                                    GameServer.m_apPlayers[i].SetTeam((int)Consts.TEAM_SPECTATORS);
                                }
                                break;
                                case 1:
                                {
                                    // move player to spectator if the reserved slots aren't filled yet, kick him otherwise
                                    int Spectators = 0;
                                    for (int j = 0; j < (int)Consts.MAX_CLIENTS; ++j)
                                        if (GameServer.m_apPlayers[j] != null && GameServer.m_apPlayers[j].GetTeam() == (int)Consts.TEAM_SPECTATORS)
                                            ++Spectators;
                                    if (Spectators >= g_Config.GetInt("SvSpectatorSlots"))
                                        Server.Kick(i, "Kicked for inactivity", -1);
                                    else
                                        GameServer.m_apPlayers[i].SetTeam((int)Consts.TEAM_SPECTATORS);
                                }
                                break;
                                case 2:
                                {
                                    // kick the player
                                    Server.Kick(i, "Kicked for inactivity", -1);
                                }
                                break;
                            }
                        }
                    }
                }
            }

            DoWincheck();
        }

        public virtual void Snap(int SnappingClient)
        {
            CNetObj_GameInfo pGameInfoObj = Server.SnapNetObj<CNetObj_GameInfo>((int)Consts.NETOBJTYPE_GAMEINFO, 0);
            if (pGameInfoObj == null)
                return;

            pGameInfoObj.m_GameFlags = m_GameFlags;
            pGameInfoObj.m_GameStateFlags = 0;
            if (m_GameOverTick != -1)
                pGameInfoObj.m_GameStateFlags |= (int)Consts.GAMESTATEFLAG_GAMEOVER;
            if (m_SuddenDeath != 0)
                pGameInfoObj.m_GameStateFlags |= (int)Consts.GAMESTATEFLAG_SUDDENDEATH;
            if (GameServer.m_World.m_Paused)
                pGameInfoObj.m_GameStateFlags |= (int)Consts.GAMESTATEFLAG_PAUSED;
            pGameInfoObj.m_RoundStartTick = m_RoundStartTick;
            pGameInfoObj.m_WarmupTimer = m_Warmup;

            pGameInfoObj.m_ScoreLimit = g_Config.GetInt("SvScorelimit");
            pGameInfoObj.m_TimeLimit = g_Config.GetInt("SvTimelimit");

            pGameInfoObj.m_RoundNum = g_Config.GetString("SvMaprotation").Length != 0 && g_Config.GetInt("SvRoundsPerMap") != 0 ? g_Config.GetInt("SvRoundsPerMap") : 0;
            pGameInfoObj.m_RoundCurrent = m_RoundCount + 1;
        }

        /*
            Function: on_entity
                Called when the map is loaded to process an entity
                in the map.

            Arguments:
                index - Entity index.
                pos - Where the entity is located in the world.

            Returns:
                bool?
        */

        public virtual bool OnEntity(MapItems tile, vec2 Pos)
        {
            int Type = -1;
            int SubType = 0;

            if (tile == MapItems.ENTITY_SPAWN)
                m_aaSpawnPoints[0, m_aNumSpawnPoints[0]++] = Pos;
            else if (tile == MapItems.ENTITY_SPAWN_RED)
                m_aaSpawnPoints[1, m_aNumSpawnPoints[1]++] = Pos;
            else if (tile == MapItems.ENTITY_SPAWN_BLUE)
                m_aaSpawnPoints[2, m_aNumSpawnPoints[2]++] = Pos;
            else if (tile == MapItems.ENTITY_ARMOR_1)
                Type = (int)Consts.POWERUP_ARMOR;
            else if (tile == MapItems.ENTITY_HEALTH_1)
                Type = (int)Consts.POWERUP_HEALTH;
            else if (tile == MapItems.ENTITY_WEAPON_SHOTGUN)
            {
                Type = (int)Consts.POWERUP_WEAPON;
                SubType = (int)Consts.WEAPON_SHOTGUN;
            }
            else if (tile == MapItems.ENTITY_WEAPON_GRENADE)
            {
                Type = (int)Consts.POWERUP_WEAPON;
                SubType = (int)Consts.WEAPON_GRENADE;
            }
            else if (tile == MapItems.ENTITY_WEAPON_RIFLE)
            {
                Type = (int)Consts.POWERUP_WEAPON;
                SubType = (int)Consts.WEAPON_RIFLE;
            }
            else if (tile == MapItems.ENTITY_POWERUP_NINJA && g_Config.GetInt("SvPowerups") != 0)
            {
                Type = (int)Consts.POWERUP_NINJA;
                SubType = (int)Consts.WEAPON_NINJA;
            }

            if (Type != -1)
            {
                CPickup pPickup = new CPickup(GameServer.m_World, Type, SubType);
                pPickup.m_Pos = Pos;
                return true;
            }

            return false;
        }

        /*
            Function: on_CCharacter_spawn
                Called when a CCharacter spawns into the game world.

            Arguments:
                chr - The CCharacter that was spawned.
        */

        public virtual void OnCharacterSpawn(CCharacter pChr)
        {
            // default health
            pChr.IncreaseHealth(10);

            // give default weapons
            pChr.GiveWeapon((int)Consts.WEAPON_HAMMER, -1);
            pChr.GiveWeapon((int)Consts.WEAPON_GUN, 10);
        }

        /*
		    Function: on_CCharacter_death
			    Called when a CCharacter in the world dies.

		    Arguments:
			    victim - The CCharacter that died.
			    killer - The player that killed it.
			    weapon - What weapon that killed it. Can be -1 for undefined
				    weapon when switching team or player suicides.
	    */

        public virtual int OnCharacterDeath(CCharacter pVictim, CPlayer pKiller, int Weapon)
        {
            // do scoreing
            if (pKiller == null || Weapon == CCharacter.WEAPON_GAME)
                return 0;
            if (pKiller == pVictim.GetPlayer())
                pVictim.GetPlayer().m_Score--; // suicide
            else
            {
                if (IsTeamplay() && pVictim.GetPlayer().GetTeam() == pKiller.GetTeam())
                    pKiller.m_Score--; // teamkill
                else
                    pKiller.m_Score++; // normal kill
            }
            if (Weapon == CCharacter.WEAPON_SELF)
                pVictim.GetPlayer().m_RespawnTick = Server.Tick() + Server.TickSpeed() * 3;
            return 0;
        }


        public virtual void OnPlayerInfoChange(CPlayer pP)
        {
            int[] aTeamColors = { 65387, 10223467 };
            if (IsTeamplay())
            {
                pP.m_TeeInfos.m_UseCustomColor = 1;
                if (pP.GetTeam() >= (int)Consts.TEAM_RED && pP.GetTeam() <= (int)Consts.TEAM_BLUE)
                {
                    pP.m_TeeInfos.m_ColorBody = aTeamColors[pP.GetTeam()];
                    pP.m_TeeInfos.m_ColorFeet = aTeamColors[pP.GetTeam()];
                }
                else
                {
                    pP.m_TeeInfos.m_ColorBody = 12895054;
                    pP.m_TeeInfos.m_ColorFeet = 12895054;
                }
            }
        }

        /*

        */

        public virtual string GetTeamName(int Team)
        {
            if (IsTeamplay())
            {
                if (Team == (int)Consts.TEAM_RED)
                    return "red team";
                if (Team == (int)Consts.TEAM_BLUE)
                    return "blue team";
            }
            else
            {
                if (Team == 0)
                    return "game";
            }

            return "spectators";
        }

        public virtual int GetAutoTeam(int NotThisID)
        {
            // this will force the auto balancer to work overtime aswell
            if (g_Config.GetInt("DbgStress") != 0)
                return 0;

            int[] aNumplayers = new int[2] { 0, 0 };
            for (int i = 0; i < (int)Consts.MAX_CLIENTS; i++)
            {
                if (GameServer.m_apPlayers[i] != null && i != NotThisID)
                {
                    if (GameServer.m_apPlayers[i].GetTeam() >= (int)Consts.TEAM_RED && GameServer.m_apPlayers[i].GetTeam() <= (int)Consts.TEAM_BLUE)
                        aNumplayers[GameServer.m_apPlayers[i].GetTeam()]++;
                }
            }

            int Team = 0;
            if (IsTeamplay())
                Team = aNumplayers[(int)Consts.TEAM_RED] > aNumplayers[(int)Consts.TEAM_BLUE] ? (int)Consts.TEAM_BLUE : (int)Consts.TEAM_RED;

            if (CanJoinTeam(Team, NotThisID))
                return Team;
            return -1;
        }

        public virtual bool CanJoinTeam(int Team, int NotThisID)
        {
            if (Team == (int)Consts.TEAM_SPECTATORS || (GameServer.m_apPlayers[NotThisID] != null && GameServer.m_apPlayers[NotThisID].GetTeam() != (int)Consts.TEAM_SPECTATORS))
                return true;

            int[] aNumplayers = new int[2] { 0, 0 };
            for (int i = 0; i < (int)Consts.MAX_CLIENTS; i++)
            {
                if (GameServer.m_apPlayers[i] != null && i != NotThisID)
                {
                    if (GameServer.m_apPlayers[i].GetTeam() >= (int)Consts.TEAM_RED && GameServer.m_apPlayers[i].GetTeam() <= (int)Consts.TEAM_BLUE)
                        aNumplayers[GameServer.m_apPlayers[i].GetTeam()]++;
                }
            }

            return aNumplayers[0] + aNumplayers[1] < g_Config.GetInt("SvMaxClients") - g_Config.GetInt("SvSpectatorSlots");
        }

        public virtual void PostReset()
        {
            for (int i = 0; i < (int)Consts.MAX_CLIENTS; i++)
            {
                if (GameServer.m_apPlayers[i] != null)
                {
                    GameServer.m_apPlayers[i].Respawn();
                    GameServer.m_apPlayers[i].m_Score = 0;
                    GameServer.m_apPlayers[i].m_ScoreStartTick = Server.Tick();
                    GameServer.m_apPlayers[i].m_RespawnTick = Server.Tick() + Server.TickSpeed() / 2;
                }
            }
        }

        private static bool IsSeparator(char c) { return c == ';' || c == ' ' || c == ',' || c == '\t'; }
    }
}

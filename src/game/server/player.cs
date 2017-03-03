using System;
using System.Runtime.InteropServices;
using vec2 = Teecsharp.vector2_float;

namespace Teecsharp
{
    public class CPlayer
    {
        private readonly CGameContext m_pGameServer;
        private readonly Localization _localization;
        
        //
        protected CCharacter m_pCharacter;
        protected bool m_Spawning;
        protected readonly int m_ClientID;
        protected int m_Team;
        protected int m_SnapTeam;

        //---------------------------------------------------------
        // this is used for snapping so we know how we can clip the view for the player
        public vec2 m_ViewPos;

        // states if the client is chatting, accessing a menu etc.
        public int m_PlayerFlags;

        // used for snapping to just update latency if the scoreboard is active
        public int[] m_aActLatency = new int[(int)Consts.MAX_CLIENTS];

        // used for spectator mode
        public int m_SpectatorID;
        public int m_ClientVersion;
        public bool m_IsReady;

        //
        public int m_Vote;
        public int m_VotePos;
        //
        public int m_LastVoteCall;
        public int m_LastVoteTry;
        public int m_LastChat;
        public int m_LastSetTeam;
        public int m_LastSetSpectatorMode;
        public int m_LastChangeInfo;
        public int m_LastEmote;
        public int m_LastKill;

        public class TeeInfos
        {
            public string m_SkinName;
            public int m_UseCustomColor;
            public int m_ColorBody;
            public int m_ColorFeet;
        }
        public TeeInfos m_TeeInfos = new TeeInfos();

        public int m_RespawnTick;
        public int m_DieTick;
        public int m_Score;
        public int m_ScoreStartTick;
        public bool m_ForceBalanced;
        public int m_LastActionTick;
        public int m_TeamChangeTick;

        public class LatestActivity
        {
            public int m_TargetX;
            public int m_TargetY;
        }
        public LatestActivity m_LatestActivity = new LatestActivity();

        public class Latency
        {
            public int m_Accum;
            public int m_AccumMin;
            public int m_AccumMax;
            public int m_Avg;
            public int m_Min;
            public int m_Max;
        }
        public Latency m_Latency = new Latency();

        public CGameContext GameServer
        {
            get { return m_pGameServer; }
        }

        public IServer Server
        {
            get { return m_pGameServer.Server; }
        }

        public CPlayer(CGameContext pGameServer, int ClientID, int Team)
        {
            m_pGameServer = pGameServer;
            m_RespawnTick = Server.Tick();
            m_DieTick = Server.Tick();
            m_ScoreStartTick = Server.Tick();
            m_pCharacter = null;
            m_ClientID = ClientID;
            m_Team = Team;
            m_SpectatorID = (int)Consts.SPEC_FREEVIEW;
            m_LastActionTick = Server.Tick();
            m_Spawning = false;

            _localization = new Localization("none", pGameServer.Languages);

            int idMap = Server.GetIdMap(ClientID);
            for (int i = 1; i < (int)Consts.VANILLA_MAX_CLIENTS; i++)
            {
                Server.IdMap[idMap + i] = -1;
            }
            Server.IdMap[idMap + 0] = ClientID;

            m_TeamChangeTick = Server.Tick();
            m_ClientVersion = (int)Consts.VERSION_VANILLA;
        }

        ~CPlayer()
        {
            m_pCharacter = null;
        }

        public void SetLanguage(string lang)
        {
            _localization.CurrentLang = lang;
        }

        public string Localize(string str, params object[] args)
        {
            if (args.Length == 0)
                return _localization.Localize(str);
            return string.Format(_localization.Localize(str), args);
        }

        public CCharacter GetCharacter()
        {
            if (m_pCharacter != null && m_pCharacter.IsAlive())
                return m_pCharacter;
            return null;
        }

        public virtual void KillCharacter(int Weapon = CCharacter.WEAPON_GAME)
        {
            if (m_pCharacter != null)
            {
                m_pCharacter.Die(m_ClientID, Weapon);
                m_pCharacter = null;
            }
        }

        public virtual void Respawn()
        {
            if (m_Team != (int)Consts.TEAM_SPECTATORS)
                m_Spawning = true;
        }

        public virtual void Tick()
        {
            if (!Server.ClientIngame(m_ClientID))
                return;

            Server.SetClientScore(m_ClientID, m_Score);

            // do latency stuff
            {
                IServer.CClientInfo info;
                if (Server.GetClientInfo(m_ClientID, out info))
                {
                    m_Latency.m_Accum += info.m_Latency;
                    m_Latency.m_AccumMax = Math.Max(m_Latency.m_AccumMax, info.m_Latency);
                    m_Latency.m_AccumMin = Math.Min(m_Latency.m_AccumMin, info.m_Latency);
                }
                // each second
                if (Server.Tick() % Server.TickSpeed() == 0)
                {
                    m_Latency.m_Avg = m_Latency.m_Accum / Server.TickSpeed();
                    m_Latency.m_Max = m_Latency.m_AccumMax;
                    m_Latency.m_Min = m_Latency.m_AccumMin;
                    m_Latency.m_Accum = 0;
                    m_Latency.m_AccumMin = 1000;
                    m_Latency.m_AccumMax = 0;
                }
            }

            if (!GameServer.m_World.m_Paused)
            {
                if (m_pCharacter == null && m_Team == (int)Consts.TEAM_SPECTATORS && m_SpectatorID == (int)Consts.SPEC_FREEVIEW)
                    m_ViewPos -= new vec2(CMath.clamp(m_ViewPos.x - m_LatestActivity.m_TargetX, -500.0f, 500.0f), CMath.clamp(m_ViewPos.y - m_LatestActivity.m_TargetY, -400.0f, 400.0f));

                if (m_pCharacter == null && m_DieTick + Server.TickSpeed() * 3 <= Server.Tick())
                    m_Spawning = true;

                if (m_pCharacter != null)
                {
                    if (m_pCharacter.IsAlive())
                    {
                        m_ViewPos = m_pCharacter.m_Pos;
                    }
                    else
                    {
                        m_pCharacter = null;
                    }
                }
                else if (m_Spawning && m_RespawnTick <= Server.Tick())
                    TryRespawn();
            }
            else
            {
                ++m_RespawnTick;
                ++m_DieTick;
                ++m_ScoreStartTick;
                ++m_LastActionTick;
                ++m_TeamChangeTick;
            }
        }

        public virtual void PostTick()
        {
            // update latency value
            if ((m_PlayerFlags & (int)Consts.PLAYERFLAG_SCOREBOARD) != 0)
            {
                for (int i = 0; i < (int)Consts.MAX_CLIENTS; ++i)
                {
                    if (GameServer.m_apPlayers[i] != null && GameServer.m_apPlayers[i].GetTeam() != (int)Consts.TEAM_SPECTATORS)
                        m_aActLatency[i] = GameServer.m_apPlayers[i].m_Latency.m_Min;
                }
            }

            // update view pos for spectators
            if (m_Team == (int)Consts.TEAM_SPECTATORS && m_SpectatorID != (int)Consts.SPEC_FREEVIEW && GameServer.m_apPlayers[m_SpectatorID] != null)
                m_ViewPos = GameServer.m_apPlayers[m_SpectatorID].m_ViewPos;
        }

        public virtual void Snap(int SnappingClient)
        {
            if (!Server.ClientIngame(m_ClientID))
                return;

            int id = m_ClientID;
            if (!Server.Translate(ref id, SnappingClient))
                return;

            CNetObj_ClientInfo pClientInfo = Server.SnapNetObj<CNetObj_ClientInfo>((int)Consts.NETOBJTYPE_CLIENTINFO, id);

            if (pClientInfo == null)
                return;

            pClientInfo.m_Country = Server.ClientCountry(m_ClientID);
            pClientInfo.m_UseCustomColor = m_TeeInfos.m_UseCustomColor;
            pClientInfo.m_ColorBody = m_TeeInfos.m_ColorBody;
            pClientInfo.m_ColorFeet = m_TeeInfos.m_ColorFeet;

            GCHelpers.StrTo4Ints(Server.ClientName(m_ClientID), ref pClientInfo.m_Name0, ref pClientInfo.m_Name1,
                ref pClientInfo.m_Name2, ref pClientInfo.m_Name3);

            GCHelpers.StrTo3Ints(Server.ClientClan(m_ClientID), ref pClientInfo.m_Clan0, ref pClientInfo.m_Clan1,
                ref pClientInfo.m_Clan2);

            GCHelpers.StrTo6Ints(m_TeeInfos.m_SkinName, ref pClientInfo.m_Skin0, ref pClientInfo.m_Skin1,
                ref pClientInfo.m_Skin2, ref pClientInfo.m_Skin3, ref pClientInfo.m_Skin4, ref pClientInfo.m_Skin5);

            CNetObj_PlayerInfo pPlayerInfo = Server.SnapNetObj<CNetObj_PlayerInfo>((int)Consts.NETOBJTYPE_PLAYERINFO, id);

            if (pPlayerInfo == null)
                return;

            pPlayerInfo.m_Latency = SnappingClient == -1 ? m_Latency.m_Min : GameServer.m_apPlayers[SnappingClient].m_aActLatency[m_ClientID];
            pPlayerInfo.m_Local = 0;
            pPlayerInfo.m_ClientID = id;
            pPlayerInfo.m_Score = m_Score;
            pPlayerInfo.m_Team = m_Team;

            if (m_ClientID == SnappingClient)
                pPlayerInfo.m_Local = 1;

            if (m_ClientID == SnappingClient && m_Team == (int)Consts.TEAM_SPECTATORS)
            {
                CNetObj_SpectatorInfo pSpectatorInfo = Server.SnapNetObj<CNetObj_SpectatorInfo>((int)Consts.NETOBJTYPE_SPECTATORINFO, m_ClientID);

                if (pSpectatorInfo == null)
                    return;

                pSpectatorInfo.m_SpectatorID = m_SpectatorID;
                pSpectatorInfo.m_X = (int)m_ViewPos.x;
                pSpectatorInfo.m_Y = (int)m_ViewPos.y;
            }
        }

        public virtual int GetTeam()
        {
            return m_Team;
        }

        public virtual void SetTeam(int Team, bool DoChatMsg = true)
        {
            // clamp the team
            Team = GameServer.Controller.ClampTeam(Team);
            if (m_Team == Team)
                return;

            string aBuf;
            if (DoChatMsg)
            {
                aBuf = string.Format("'{0}' joined the {1}", Server.ClientName(m_ClientID), GameServer.Controller.GetTeamName(Team));
                GameServer.SendChat(-1, CGameContext.CHAT_ALL, aBuf);
            }

            KillCharacter();

            m_Team = Team;
            m_LastActionTick = Server.Tick();
            m_SpectatorID = (int)Consts.SPEC_FREEVIEW;
            // we got to wait 0.5 secs before respawning
            m_RespawnTick = Server.Tick() + Server.TickSpeed() / 2;
            aBuf = string.Format("team_join player='{0}:{1}' m_Team={2}", m_ClientID, Server.ClientName(m_ClientID), m_Team);
            GameServer.Console.Print(IConsole.OUTPUT_LEVEL_DEBUG, "game", aBuf);

            GameServer.Controller.OnPlayerInfoChange(GameServer.m_apPlayers[m_ClientID]);

            if (Team == (int)Consts.TEAM_SPECTATORS)
            {
                // update spectator modes
                for (int i = 0; i < (int)Consts.MAX_CLIENTS; ++i)
                {
                    if (GameServer.m_apPlayers[i] != null && GameServer.m_apPlayers[i].m_SpectatorID == m_ClientID)
                        GameServer.m_apPlayers[i].m_SpectatorID = (int)Consts.SPEC_FREEVIEW;
                }
            }
        }

        public int GetCID()
        {
            return m_ClientID;
        }

        public virtual void TryRespawn()
        {
            vec2 SpawnPos = new vec2();

            if (!GameServer.Controller.CanSpawn(m_Team, ref SpawnPos))
                return;

            m_Spawning = false;
            m_pCharacter = new CCharacter(GameServer.m_World);
            m_pCharacter.Spawn(this, SpawnPos);
            GameServer.CreatePlayerSpawn(SpawnPos);
        }
        
        // TODO
        public virtual void OnDirectInput(CNetObj_PlayerInput NewInput)
        {
            if ((NewInput.m_PlayerFlags & (int)Consts.PLAYERFLAG_CHATTING) != 0)
            {
                // skip the input if chat is active
                if ((m_PlayerFlags & (int)Consts.PLAYERFLAG_CHATTING) != 0)
                    return;

                // reset input
                m_pCharacter?.ResetInput();

                m_PlayerFlags = NewInput.m_PlayerFlags;
                return;
            }

            m_PlayerFlags = NewInput.m_PlayerFlags;

            m_pCharacter?.OnDirectInput(NewInput);

            if (m_pCharacter == null && m_Team != (int)Consts.TEAM_SPECTATORS && (NewInput.m_Fire & 1) != 0)
                m_Spawning = true;

            // check for activity
            if (NewInput.m_Direction != 0 || m_LatestActivity.m_TargetX != NewInput.m_TargetX ||
                m_LatestActivity.m_TargetY != NewInput.m_TargetY || NewInput.m_Jump != 0 ||
                (NewInput.m_Fire & 1) != 0 || NewInput.m_Hook != 0)
            {
                m_LatestActivity.m_TargetX = NewInput.m_TargetX;
                m_LatestActivity.m_TargetY = NewInput.m_TargetY;
                m_LastActionTick = Server.Tick();
            }
        }

        public virtual void OnPredictedInput(CNetObj_PlayerInput NewInput)
        {
            // skip the input if chat is active
            if ((m_PlayerFlags & (int)Consts.PLAYERFLAG_CHATTING) != 0 && (NewInput.m_PlayerFlags & (int)Consts.PLAYERFLAG_CHATTING) != 0)
                return;

            m_pCharacter?.OnPredictedInput(NewInput);
        }

        public void FakeSnap(int SnappingClient)
        {
            IServer.CClientInfo info;
            Server.GetClientInfo(SnappingClient, out info);

            if (GameServer.m_apPlayers[SnappingClient] != null &&
                GameServer.m_apPlayers[SnappingClient].m_ClientVersion >= (int)Consts.VERSION_DDNET_OLD)
                return;

            int id = (int)Consts.VANILLA_MAX_CLIENTS - 1;

            CNetObj_ClientInfo pClientInfo = Server.SnapNetObj<CNetObj_ClientInfo>((int)Consts.NETOBJTYPE_CLIENTINFO, id);

            if (pClientInfo == null)
                return;

            GCHelpers.StrTo4Ints(" ", ref pClientInfo.m_Name0, ref pClientInfo.m_Name1,
                ref pClientInfo.m_Name2, ref pClientInfo.m_Name3);

            GCHelpers.StrTo3Ints(Server.ClientClan(m_ClientID), ref pClientInfo.m_Clan0, ref pClientInfo.m_Clan1,
                ref pClientInfo.m_Clan2);

            GCHelpers.StrTo6Ints(m_TeeInfos.m_SkinName, ref pClientInfo.m_Skin0, ref pClientInfo.m_Skin1,
                ref pClientInfo.m_Skin2, ref pClientInfo.m_Skin3, ref pClientInfo.m_Skin4, ref pClientInfo.m_Skin5);
        }

        public virtual void OnDisconnect(string pReason)
        {
            KillCharacter();

            if (Server.ClientIngame(m_ClientID))
            {
                string aBuf;
                if (!string.IsNullOrEmpty(pReason))
                    aBuf = string.Format("'{0}' has left the game ({1})", Server.ClientName(m_ClientID), pReason);
                else
                    aBuf = string.Format("'{0}' has left the game", Server.ClientName(m_ClientID));
                GameServer.SendChat(-1, CGameContext.CHAT_ALL, aBuf);

                aBuf = string.Format("leave player='{0}:{1}'", m_ClientID, Server.ClientName(m_ClientID));
                GameServer.Console.Print(IConsole.OUTPUT_LEVEL_STANDARD, "game", aBuf);
            }
        }
    }
}

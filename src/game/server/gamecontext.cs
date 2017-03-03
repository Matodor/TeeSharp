using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using vec2 = Teecsharp.vector2_float;

namespace Teecsharp
{
    /*
	    Tick
		    Game Context (CGameContext::tick)
			    Game World (GAMEWORLD::tick)
				    Reset world if requested (GAMEWORLD::reset)
				    All entities in the world (ENTITY::tick)
				    All entities in the world (ENTITY::tick_defered)
				    Remove entities marked for deletion (GAMEWORLD::remove_entities)
			    Game Controller (GAMECONTROLLER::tick)
			    All players (CPlayer::tick)
                
	    Snap
		    Game Context (CGameContext::snap)
			    Game World (GAMEWORLD::snap)
				    All entities in the world (ENTITY::snap)
			    Game Controller (GAMECONTROLLER::snap)
			    Events handler (EVENT_HANDLER::snap)
			    All players (CPlayer::snap)

    */

    public class CGameContext : IGameServer
    {
        /*  EVENTS */
        public event Action<CPlayer> OnClientConnectedEvent;
        public event Action<CPlayer> OnClientDropEvent;


        public Languages Languages { get { return m_Languages; } }
        public CServer Server { get { return (CServer) m_pServer; } }
        public IConsole Console { get { return m_pConsole; } }
        public CCollision Collision { get { return m_Collision; } }
        public CTuningParams Tuning { get { return m_Tuning; } }
        public IGameController Controller { get { return m_pController; } }
        public ChatController ChatController { get { return m_ChatController; } }

        public readonly CEventHandler m_Events;
        public readonly CPlayer[] m_apPlayers;
        public readonly CGameWorld m_World;

        public const int
            VOTE_ENFORCE_UNKNOWN = 0,
            VOTE_ENFORCE_NO = 1,
            VOTE_ENFORCE_YES = 2;

        public int m_LockTeams;

        public const int
            CHAT_ALL = -2,
            CHAT_SPEC = -1,
            CHAT_RED = 0,
            CHAT_BLUE = 1,
            MAX_CLIENTS = (int)Consts.MAX_CLIENTS;

        private IGameController m_pController;
        private IServer m_pServer;
        private IConsole m_pConsole;
        private readonly CLayers m_Layers;
        private readonly CCollision m_Collision;
        private readonly CNetObjHandler m_NetObjHandler;
        private CTuningParams m_Tuning;
        private readonly Languages m_Languages;
        private readonly ChatController m_ChatController;

        public CGameContext()
        {
            m_pServer = null;

            m_Languages = new Languages();
            m_Layers = new CLayers();
            m_Collision = new CCollision(this);
            m_NetObjHandler = new CNetObjHandler();
            m_World = new CGameWorld();
            m_Events = new CEventHandler();
            m_apPlayers = new CPlayer[(int)Consts.MAX_CLIENTS];
            m_Tuning = new CTuningParams();
            m_ChatController = new ChatController(this);

            for (int i = 0; i < MAX_CLIENTS; i++)
                m_apPlayers[i] = null;

            m_pController = null;
        }

        ~CGameContext()
        {
            for (int i = 0; i < MAX_CLIENTS; i++)
                m_apPlayers[i] = null;
        }

        public void Clear()
        {
            //CHeap pVoteOptionHeap = m_pVoteOptionHeap;
            //CVoteOptionServer pVoteOptionFirst = m_pVoteOptionFirst;
            //CVoteOptionServer pVoteOptionLast = m_pVoteOptionLast;
            //int NumVoteOptions = m_NumVoteOptions;
            //CTuningParams Tuning = m_Tuning;

            //m_Resetting = true;
            //~CGameContext();
            //mem_zero(this, sizeof(*this));
            //new (this) CGameContext(RESET);

            //m_pVoteOptionHeap = pVoteOptionHeap;
            //m_pVoteOptionFirst = pVoteOptionFirst;
            //m_pVoteOptionLast = pVoteOptionLast;
            //m_NumVoteOptions = NumVoteOptions;
            //m_Tuning = Tuning;
        }

        // helper functions
        public CCharacter GetPlayerChar(int ClientID)
        {
            if (ClientID < 0 || ClientID >= m_apPlayers.Length)
                return null;
            return m_apPlayers[ClientID]?.GetCharacter();
        }

        // helper functions
        public void CreateDamageInd(vec2 Pos, float Angle, int Amount)
        {
            float a = 3 * 3.14159f / 2 + Angle;
            float s = a - CMath.pi / 3;
            float e = a + CMath.pi / 3;
            for (int i = 0; i < Amount; i++)
            {
                float f = CMath.mix(s, e, (float)(i + 1) / (Amount + 2));
                CNetEvent_DamageInd pEvent = m_Events.Create<CNetEvent_DamageInd>((int)Consts.NETEVENTTYPE_DAMAGEIND);
                if (pEvent != null)
                {
                    pEvent.m_X = (int)Pos.x;
                    pEvent.m_Y = (int)Pos.y;
                    pEvent.m_Angle = (int)(f * 256.0f);
                }
            }
        }

        public void CreateExplosion(vec2 Pos, int Owner, int Weapon, bool NoDamage)
        {
            // create the event
            CNetEvent_Explosion pEvent = m_Events.Create<CNetEvent_Explosion>((int)Consts.NETEVENTTYPE_EXPLOSION);
            if (pEvent != null)
            {
                pEvent.m_X = (int)Pos.x;
                pEvent.m_Y = (int)Pos.y;
            }

            if (!NoDamage)
            {
                // deal damage
                CEntity[] apEnts = new CEntity[MAX_CLIENTS];
                float Radius = 135.0f;
                float InnerRadius = 48.0f;
                int Num = m_World.FindEntities(Pos, Radius, ref apEnts, MAX_CLIENTS, CGameWorld.ENTTYPE_CHARACTER);
                for (int i = 0; i < Num; i++)
                {
                    vec2 Diff = apEnts[i].m_Pos - Pos;
                    vec2 ForceDir = new vec2(0, 1);
                    float l = VMath.length(Diff);
                    if (l > 0)
                        ForceDir = VMath.normalize(Diff);
                    l = 1 - CMath.clamp((l - InnerRadius) / (Radius - InnerRadius), 0.0f, 1.0f);
                    float Dmg = 6 * l;
                    if ((int)Dmg != 0)
                        ((CCharacter)apEnts[i]).TakeDamage(ForceDir * Dmg * 2, (int)Dmg, Owner, Weapon);
                }
            }
        }

        public void CreateHammerHit(vec2 Pos)
        {
            // create the event
            CNetEvent_HammerHit pEvent = m_Events.Create<CNetEvent_HammerHit>((int)Consts.NETEVENTTYPE_HAMMERHIT);
            if (pEvent != null)
            {
                pEvent.m_X = (int)Pos.x;
                pEvent.m_Y = (int)Pos.y;
            }
        }

        public void CreatePlayerSpawn(vec2 Pos)
        {
            // create the event
            CNetEvent_Spawn ev = m_Events.Create<CNetEvent_Spawn>((int)Consts.NETEVENTTYPE_SPAWN);
            if (ev != null)
            {
                ev.m_X = (int)Pos.x;
                ev.m_Y = (int)Pos.y;
            }
        }

        public void CreateDeath(vec2 Pos, int ClientID)
        {
            // create the event
            CNetEvent_Death pEvent = m_Events.Create<CNetEvent_Death>((int)Consts.NETEVENTTYPE_DEATH);
            if (pEvent != null)
            {
                pEvent.m_X = (int)Pos.x;
                pEvent.m_Y = (int)Pos.y;
                pEvent.m_ClientID = ClientID;
            }
        }

        public void CreateSound(vec2 Pos, int Sound, int Mask = -1)
        {
            if (Sound < 0)
                return;

            // create a sound
            CNetEvent_SoundWorld pEvent = m_Events.Create<CNetEvent_SoundWorld>((int)Consts.NETEVENTTYPE_SOUNDWORLD, Mask);

            if (pEvent == null)
                return;

            pEvent.m_X = (int)Pos.x;
            pEvent.m_Y = (int)Pos.y;
            pEvent.m_SoundID = Sound;
        }

        public void CreateSoundGlobal(int Sound, int Target = -1)
        {
            if (Sound < 0)
                return;

            CNetMsg_Sv_SoundGlobal Msg = new CNetMsg_Sv_SoundGlobal();
            Msg.m_SoundID = Sound;
            if (Target == -2)
                Server.SendPackMsg(Msg, (int)Consts.MSGFLAG_NOSEND, -1);
            else
            {
                int Flag = (int)Consts.MSGFLAG_VITAL;
                if (Target != -1)
                    Flag |= (int)Consts.MSGFLAG_NORECORD;
                Server.SendPackMsg(Msg, Flag, Target);
            }
        }

        // network
        public void SendChatTarget(int To, string pText)
        {
            if (m_apPlayers[To] == null)
                return;

            CNetMsg_Sv_Chat Msg = new CNetMsg_Sv_Chat
            {
                m_Team = 0,
                m_ClientID = -1,
                m_pMessage = pText
            };
            Server.SendPackMsg(Msg, (int)Consts.MSGFLAG_VITAL, To);
        }

        public void SendChat(int ChatterClientID, int Team, string pText)
        {
            string aBuf;
            if (ChatterClientID >= 0 && ChatterClientID < MAX_CLIENTS)
                aBuf = string.Format("{0}:{1}:{2}: {3}", ChatterClientID, Team, Server.ClientName(ChatterClientID), pText);
            else
                aBuf = string.Format("*** {0}", pText);
            Console.Print(IConsole.OUTPUT_LEVEL_ADDINFO, Team != CHAT_ALL ? "teamchat" : "chat", aBuf);

            if (Team == CHAT_ALL)
            {
                CNetMsg_Sv_Chat Msg = new CNetMsg_Sv_Chat
                {
                    m_Team = 0,
                    m_ClientID = ChatterClientID,
                    m_pMessage = pText
                };
                Server.SendPackMsg(Msg, (int)Consts.MSGFLAG_VITAL, -1);
            }
            else
            {
                CNetMsg_Sv_Chat Msg = new CNetMsg_Sv_Chat
                {
                    m_Team = 1,
                    m_ClientID = ChatterClientID,
                    m_pMessage = pText
                };

                // pack one for the recording only
                //Server.SendPackMsg(Msg, (int)Consts.MSGFLAG_VITAL | (int)Consts.MSGFLAG_NOSEND, -1);

                if (Team == CHAT_SPEC)
                {
                    // send to the clients
                    for (int i = 0; i < MAX_CLIENTS; i++)
                    {
                        if (m_apPlayers[i] != null && m_apPlayers[i].GetTeam() == Team)
                            Server.SendPackMsg(Msg, (int) Consts.MSGFLAG_VITAL | (int) Consts.MSGFLAG_NORECORD, i);
                    }
                }
                else
                {
                    for (int i = 0; i < MAX_CLIENTS; i++)
                    {
                        if (m_apPlayers[i] != null && m_apPlayers[i].GetTeam() == m_apPlayers[ChatterClientID].GetTeam())
                            Server.SendPackMsg(Msg, (int)Consts.MSGFLAG_VITAL | (int)Consts.MSGFLAG_NORECORD, i);
                    }
                }
            }
        }

        public void SendEmoticon(int ClientID, int Emoticon)
        {
            CNetMsg_Sv_Emoticon Msg = new CNetMsg_Sv_Emoticon
            {
                m_ClientID = ClientID,
                m_Emoticon = Emoticon
            };
            Server.SendPackMsg(Msg, (int)Consts.MSGFLAG_VITAL, -1);
        }

        public void SendWeaponPickup(int ClientID, int Weapon)
        {
            CNetMsg_Sv_WeaponPickup Msg = new CNetMsg_Sv_WeaponPickup();
            Msg.m_Weapon = Weapon;
            Server.SendPackMsg(Msg, (int)Consts.MSGFLAG_VITAL, ClientID);
        }

        public void SendBroadcast(string pText, int ClientID)
        {
            CNetMsg_Sv_Broadcast Msg = new CNetMsg_Sv_Broadcast {m_pMessage = pText};
            Server.SendPackMsg(Msg, (int)Consts.MSGFLAG_VITAL, ClientID);
        }
        
        void CheckPureTuning()
        {
            // might not be created yet during start up
            if (m_pController == null)
                return;

            if (Controller.m_pGameType == "DM" ||
                Controller.m_pGameType == "TDM" ||
                Controller.m_pGameType == "CTF")
            {
                CTuningParams p = new CTuningParams();

                bool error = false;
                foreach (var param in p.Params)
                {
                    if (m_Tuning.GetParam(param.Key).IntValue != param.Value.IntValue)
                    {
                        error = true;
                        break;
                    }
                }

                if (error)
                {
                    Console.Print(IConsole.OUTPUT_LEVEL_STANDARD, "server", "resetting tuning due to pure server");
                    m_Tuning = p;
                }
            }
        }

        public void SendTuningParams(int ClientID)
        {
            //CheckPureTuning();
            CMsgPacker Msg = new CMsgPacker((int)Consts.NETMSGTYPE_SV_TUNEPARAMS);
            foreach (var param in m_Tuning.Params)
                Msg.AddInt(param.Value.IntValue);
            Server.SendMsg(Msg, (int)Consts.MSGFLAG_VITAL, ClientID);
        }


        // voting
        void StartVote(string pDesc, string pCommand, string pReason)
        {

        }

        void EndVote()
        {

        }

        void SendVoteSet(int ClientID)
        {

        }

        void SendVoteStatus(int ClientID, int Total, int Yes, int No)
        {

        }

        void AbortVoteKickOnDisconnect(int ClientID)
        {

        }

        public void ClearVotes(int ClientID)
        {

        }

        private static void ConTuneParam(CConsoleResult result, object pUserData)
        {

        }

        private static void ConTuneReset(CConsoleResult result, object pUserData)
        {

        }

        private static void ConTuneDump(CConsoleResult result, object pUserData)
        {
            CGameContext pSelf = (CGameContext)pUserData;
            foreach (var param in pSelf.Tuning.Params)
            {
                float v = param.Value.IntValue / 100f;
                var aBuf = string.Format("{0} {1}", param.Value.ScriptName, v);
                pSelf.Console.Print(IConsole.OUTPUT_LEVEL_STANDARD, "tuning", aBuf);
            }
        }

        private static void ConPause(CConsoleResult result, object pUserData)
        {

        }

        private static void ConChangeMap(CConsoleResult result, object pUserData)
        {

        }

        private static void ConRestart(CConsoleResult result, object pUserData)
        {

        }

        private static void ConBroadcast(CConsoleResult result, object pUserData)
        {

        }
        private static void ConSay(CConsoleResult result, object pUserData)
        {

        }

        private static void ConSetTeam(CConsoleResult result, object pUserData)
        {
  
        }

        private static void ConSetTeamAll(CConsoleResult result, object pUserData)
        {

        }

        private static void ConSwapTeams(CConsoleResult result, object pUserData)
        {

        }

        private static void ConShuffleTeams(CConsoleResult result, object pUserData)
        {

        }

        private static void ConLockTeams(CConsoleResult result, object pUserData)
        {

        }

        private static void ConAddVote(CConsoleResult result, object pUserData)
        {

        }

        private static void ConRemoveVote(CConsoleResult result, object pUserData)
        {

        }

        private static void ConForceVote(CConsoleResult result, object pUserData)
        {

        }

        private static void ConClearVotes(CConsoleResult result, object pUserData)
        {

        }

        private static void ConVote(CConsoleResult result, object pUserData)
        {

        }

        /*private static void ConchainSpecialMotdupdate(CConsoleResult result, object pUserData,
            IConsole.FCommandCallback pfnCallback, object pCallbackUserData)
        {

        }*/

        public override void OnInit()
        {
            m_pServer = Kernel.RequestInterface<IServer>();
            m_pConsole = Kernel.RequestInterface<IConsole>();
            m_World.SetGameServer(this);
            m_Events.SetGameServer(this);

            m_Layers.Init(Kernel);
            m_Collision.Init(m_Layers);

            // select gametype
            m_Languages.LoadLanguages();
            m_pController = new CGameControllerDM(this);

            CMapItemLayerTilemap pTileMap = m_Layers.GameLayer();
            for (int y = 0; y < pTileMap.m_Height; y++)
            {
                for (int x = 0; x < pTileMap.m_Width; x++)
                {
                    int Index = m_Collision.GetTileAtIndex(y * pTileMap.m_Width + x).m_Index;
                    vec2 Pos = new vec2(x * 32.0f + 16.0f, y * 32.0f + 16.0f);

                    if (Index >= (int) MapItems.ENTITY_OFFSET)
                        Controller.OnEntity((MapItems)(Index - (int) MapItems.ENTITY_OFFSET), Pos);
                }
            }
        }

        public override void OnConsoleInit()
        {
            OnChatInit();

            m_pServer = Kernel.RequestInterface<IServer>();
            m_pConsole = Kernel.RequestInterface<IConsole>();

            Console.Register("tune", "si", CServer.CFGFLAG_SERVER, ConTuneParam, this, "Tune variable to value");
            Console.Register("tune_reset", "", CServer.CFGFLAG_SERVER, ConTuneReset, this, "Reset tuning");
            Console.Register("tune_dump", "", CServer.CFGFLAG_SERVER, ConTuneDump, this, "Dump tuning");

            Console.Register("pause", "", CServer.CFGFLAG_SERVER, ConPause, this, "Pause/unpause game");
            Console.Register("change_map", "", CServer.CFGFLAG_SERVER | CServer.CFGFLAG_STORE, ConChangeMap, this, "Change map");
            Console.Register("restart", "", CServer.CFGFLAG_SERVER | CServer.CFGFLAG_STORE, ConRestart, this, "Restart in x seconds (0 = abort)");
            Console.Register("broadcast", "s", CServer.CFGFLAG_SERVER, ConBroadcast, this, "Broadcast message");
            Console.Register("say", "s", CServer.CFGFLAG_SERVER, ConSay, this, "Say in chat");
            Console.Register("set_team", "ii", CServer.CFGFLAG_SERVER, ConSetTeam, this, "Set team of player to team");
            Console.Register("set_team_all", "i", CServer.CFGFLAG_SERVER, ConSetTeamAll, this, "Set team of all players to team");
            Console.Register("swap_teams", "", CServer.CFGFLAG_SERVER, ConSwapTeams, this, "Swap the current teams");
            Console.Register("shuffle_teams", "", CServer.CFGFLAG_SERVER, ConShuffleTeams, this, "Shuffle the current teams");
            Console.Register("lock_teams", "", CServer.CFGFLAG_SERVER, ConLockTeams, this, "Lock/unlock teams");

            Console.Register("add_vote", "ss", CServer.CFGFLAG_SERVER, ConAddVote, this, "Add a voting option");
            Console.Register("remove_vote", "s", CServer.CFGFLAG_SERVER, ConRemoveVote, this, "remove a voting option");
            Console.Register("force_vote", "ss", CServer.CFGFLAG_SERVER, ConForceVote, this, "Force a voting option");
            Console.Register("clear_votes", "", CServer.CFGFLAG_SERVER, ConClearVotes, this, "Clears the voting options");
            Console.Register("vote", "s", CServer.CFGFLAG_SERVER, ConVote, this, "Force a vote to yes/no");

            //Console.Chain("sv_motd", ConchainSpecialMotdupdate, this);
        }

        public bool ParseClientID(int clientId)
        {
            return clientId >= 0 && clientId < m_apPlayers.Length && m_apPlayers[clientId] != null;
        }
        
        public void CreateStar(vec2 pos, float angle)
        {
            float f = CMath.mix(800, -800, angle/360);

            CNetEvent_DamageInd pEvent = m_Events.Create<CNetEvent_DamageInd>((int)Consts.NETEVENTTYPE_DAMAGEIND);
            if (pEvent != null)
            {
                pEvent.m_X = (int)pos.x;
                pEvent.m_Y = (int)pos.y;
                pEvent.m_Angle = (int)f;
            }
        }

        private void TestCommand(ChatResult result, object data)
        {
            SendChat(result.ClientID, CHAT_ALL, result.GetString(0));
        }

        public void OnChatInit()
        {
            ChatController.Register("test", "s", TestCommand);
        }

        public override void OnShutdown()
        {
            m_World.Reset();
            m_pController = null;
            Clear();
        }

        public override void OnTick()
        {
            // check tuning
            //CheckPureTuning();

            // copy tuning
            //m_World.m_Core.m_Tuning = m_Tuning;
            m_World.Tick();

            //if(world.paused) // make sure that the game object always updates
            Controller.Tick();

            for (int i = 0; i < MAX_CLIENTS; i++)
            {
                if (m_apPlayers[i] != null)
                {
                    m_apPlayers[i].Tick();
                    m_apPlayers[i].PostTick();
                }
            }
        }

        public override void OnPreSnap()
        {

        }

        public override void OnSnap(int ClientID)
        {
            m_World.Snap(ClientID);
            Controller.Snap(ClientID);
            m_Events.Snap(ClientID);

            for (int i = 0; i < MAX_CLIENTS; i++)
            {
                if (m_apPlayers[i] != null)
                {
                    m_apPlayers[i].Snap(ClientID);
                }
            }

            m_apPlayers[ClientID].FakeSnap(ClientID);
        }

        public override void OnPostSnap()
        {
            m_Events.Clear();
        }

        public bool ValidateString(string input, string validChars)
        {
            return input.All(validChars.Contains);
        }
         
        //test 2
        public override void OnMessage(int MsgID, CUnpacker pUnpacker, int ClientID)
        {
            object pMsgSource = null;
            if (!m_NetObjHandler.SecureUnpackMsg(MsgID, pUnpacker, ref pMsgSource))
            {
                string aBuf = string.Format("dropped weird message '{0}' ({1}), failed on '{2}'", m_NetObjHandler.GetMsgName(MsgID), 
                    MsgID, m_NetObjHandler.FailedMsgOn());
                Console.Print(IConsole.OUTPUT_LEVEL_DEBUG, "server", aBuf);
                return;
            }

            CPlayer pPlayer = m_apPlayers[ClientID];
            if (Server.ClientIngame(ClientID))
            {
                if (MsgID == (int)Consts.NETMSGTYPE_CL_SAY)
                {
                    if (g_Config.GetInt("SvSpamprotection") != 0 && pPlayer.m_LastChat != 0 && 
                        pPlayer.m_LastChat + Server.TickSpeed() > Server.Tick())
                        return;

                    CNetMsg_Cl_Say pMsg = (CNetMsg_Cl_Say) pMsgSource;
                    if (string.IsNullOrWhiteSpace(pMsg.m_pMessage))
                    {
                        pPlayer.m_LastChat = Server.Tick();
                        return;
                    }

                    int Team = pMsg.m_Team != 0 ? pPlayer.GetTeam() : CHAT_ALL;
                    string message = pMsg.m_pMessage;
                    if (!string.IsNullOrWhiteSpace(message) && message.TrimStart(' ')[0] == '/')
                        message = pMsg.m_pMessage.TrimStart(' ');

                    // trim right and set maximum length to 128 utf8-characters
                    int Length = message.Length;

                    // drop empty and autocreated spam messages (more than 16 characters per second)

                    if (Length == 0 || (g_Config.GetInt("SvSpamprotection") != 0 && pPlayer.m_LastChat != 0 &&
                        pPlayer.m_LastChat + Server.TickSpeed()*((15 + Length)/16) > Server.Tick()))
                        return;
                    

                    pPlayer.m_LastChat = Server.Tick();

                    if (!m_ChatController.OnChat(ClientID, message))
                    {
                        if (message[0] == '/')
                            SendChatTarget(ClientID, pPlayer.Localize("ERROR: Command not found"));
                        else
                        {
                            SendChat(ClientID, Team, message);
                        }
                    }
                }
                else if (MsgID == (int)Consts.NETMSGTYPE_CL_CALLVOTE)
                {
                    CNetMsg_Cl_CallVote pMsg = (CNetMsg_Cl_CallVote) pMsgSource;
                }
                else if (MsgID == (int)Consts.NETMSGTYPE_CL_ISDDNET)
                {
                    int Version = pUnpacker.GetInt();
                    if (pUnpacker.Error())
                    {
                        if (pPlayer.m_ClientVersion < (int)Consts.VERSION_DDRACE)
                            pPlayer.m_ClientVersion = (int)Consts.VERSION_DDRACE;
                    }
                    else
                        pPlayer.m_ClientVersion = Version;

                    string aBuf = string.Format("{0} using Custom Client {1}", ClientID, pPlayer.m_ClientVersion);
                    CSystem.dbg_msg_clr("DDNet", aBuf, ConsoleColor.DarkYellow);
                }
                else if (MsgID == (int)Consts.NETMSGTYPE_CL_VOTE)
                {
                    CNetMsg_Cl_Vote vote = (CNetMsg_Cl_Vote) pMsgSource;
                }
                else if (MsgID == (int)Consts.NETMSGTYPE_CL_SETTEAM && !m_World.m_Paused)
                {
                    CNetMsg_Cl_SetTeam pMsg = (CNetMsg_Cl_SetTeam) pMsgSource;
                  
                    if (pMsg.m_Team != (int)Consts.TEAM_SPECTATORS && m_LockTeams != 0)
                    {
                        pPlayer.m_LastSetTeam = Server.Tick();
                        SendBroadcast("Teams are locked", ClientID);
                        return;
                    }

                    if (pPlayer.m_TeamChangeTick > Server.Tick())
                    {
                        pPlayer.m_LastSetTeam = Server.Tick();
                        int TimeLeft = (pPlayer.m_TeamChangeTick - Server.Tick()) / Server.TickSpeed();
                        string aBuf = string.Format("Time to wait before changing team: {0}:{1}", TimeLeft / 60, TimeLeft % 60);
                        SendBroadcast(aBuf, ClientID);
                        return;
                    }

                    // Switch team on given client and kill/respawn him
                    if (Controller.CanJoinTeam(pMsg.m_Team, ClientID))
                    {
                        if (Controller.CanChangeTeam(pPlayer, pMsg.m_Team))
                        {
                            pPlayer.m_LastSetTeam = Server.Tick();
                            //if (pPlayer.GetTeam() == (int)Consts.TEAM_SPECTATORS || pMsg.m_Team == (int)Consts.TEAM_SPECTATORS)
                            //    m_VoteUpdate = true;
                            pPlayer.SetTeam(pMsg.m_Team);
                            Controller.CheckTeamBalance();
                            pPlayer.m_TeamChangeTick = Server.Tick();
                        }
                        else
                            SendBroadcast("Teams must be balanced, please join other team", ClientID);
                    }
                    else
                    {
                        string aBuf = string.Format("Only {0} active players are allowed", Server.MaxClients() - g_Config.GetInt("SvSpectatorSlots"));
                        SendBroadcast(aBuf, ClientID);
                    }
                }
                else if (MsgID == (int)Consts.NETMSGTYPE_CL_SETSPECTATORMODE && !m_World.m_Paused)
                {
                    CNetMsg_Cl_SetSpectatorMode pMsg = (CNetMsg_Cl_SetSpectatorMode) pMsgSource;

                    if (pPlayer.GetTeam() != (int)Consts.TEAM_SPECTATORS || pPlayer.m_SpectatorID == pMsg.m_SpectatorID || ClientID == pMsg.m_SpectatorID ||
                        (g_Config.GetInt("SvSpamprotection") != 0 && pPlayer.m_LastSetSpectatorMode != 0 && pPlayer.m_LastSetSpectatorMode + Server.TickSpeed() * 3 > Server.Tick()))
                        return;

                    pPlayer.m_LastSetSpectatorMode = Server.Tick();
                    if (pMsg.m_SpectatorID != (int)Consts.SPEC_FREEVIEW && (m_apPlayers[pMsg.m_SpectatorID] == null || m_apPlayers[pMsg.m_SpectatorID].GetTeam() == (int)Consts.TEAM_SPECTATORS))
                        SendChatTarget(ClientID, "Invalid spectator id used");
                    else
                        pPlayer.m_SpectatorID = pMsg.m_SpectatorID;
                }
                else if (MsgID == (int)Consts.NETMSGTYPE_CL_CHANGEINFO)
                {
                    if (g_Config.GetInt("SvSpamprotection") != 0 && pPlayer.m_LastChangeInfo != 0 && pPlayer.m_LastChangeInfo + Server.TickSpeed() * 5 > Server.Tick())
                        return;

                    CNetMsg_Cl_ChangeInfo pMsg = (CNetMsg_Cl_ChangeInfo) pMsgSource;
                    pPlayer.m_LastChangeInfo = Server.Tick();

                    // set infos
                    string aOldName = Server.ClientName(ClientID);
                    Server.SetClientName(ClientID, pMsg.m_pName);

                    if (aOldName != Server.ClientName(ClientID))
                    {
                        string aChatText = string.Format("'{0}' changed name to '{1}'", aOldName, Server.ClientName(ClientID));
                        SendChat(-1, CHAT_ALL, aChatText);
                    }

                    Server.SetClientClan(ClientID, pMsg.m_pClan);
                    Server.SetClientCountry(ClientID, pMsg.m_Country);
                    pPlayer.m_TeeInfos.m_SkinName = pMsg.m_pSkin;
                    pPlayer.m_TeeInfos.m_UseCustomColor = pMsg.m_UseCustomColor;
                    pPlayer.m_TeeInfos.m_ColorBody = pMsg.m_ColorBody;
                    pPlayer.m_TeeInfos.m_ColorFeet = pMsg.m_ColorFeet;
                    Controller.OnPlayerInfoChange(pPlayer);
                }
                else if (MsgID == (int)Consts.NETMSGTYPE_CL_EMOTICON && !m_World.m_Paused)
                {
                    CNetMsg_Cl_Emoticon pMsg = (CNetMsg_Cl_Emoticon) pMsgSource;

                    if (g_Config.GetInt("SvSpamprotection") != 0 && pPlayer.m_LastEmote != 0 && pPlayer.m_LastEmote + Server.TickSpeed() * 3 > Server.Tick())
                        return;

                    pPlayer.m_LastEmote = Server.Tick();
                    SendEmoticon(ClientID, pMsg.m_Emoticon);
                }
                else if (MsgID == (int)Consts.NETMSGTYPE_CL_KILL && !m_World.m_Paused)
                {
                    /*
                    if (pPlayer.m_LastKill != 0 && pPlayer.m_LastKill + Server.TickSpeed() * 3 > Server.Tick())
                        return;

                    pPlayer.m_LastKill = Server.Tick();
                    pPlayer.KillCharacter(CCharacter.WEAPON_SELF);
                    */
                }
            }
            else
            {
                if (MsgID == (int)Consts.NETMSGTYPE_CL_STARTINFO)
                {
                    if (pPlayer.m_IsReady)
                        return;

                    CNetMsg_Cl_StartInfo pMsg = (CNetMsg_Cl_StartInfo) pMsgSource;
                    pPlayer.m_LastChangeInfo = Server.Tick();

                    // set start infos
                    Server.SetClientName(ClientID, pMsg.m_pName);
                    Server.SetClientClan(ClientID, pMsg.m_pClan);
                    Server.SetClientCountry(ClientID, pMsg.m_Country);
                    pPlayer.m_TeeInfos.m_SkinName = pMsg.m_pSkin;
                    pPlayer.m_TeeInfos.m_UseCustomColor = pMsg.m_UseCustomColor;
                    pPlayer.m_TeeInfos.m_ColorBody = pMsg.m_ColorBody;
                    pPlayer.m_TeeInfos.m_ColorFeet = pMsg.m_ColorFeet;
                    Controller.OnPlayerInfoChange(pPlayer);

                    // send vote options
                    CNetMsg_Sv_VoteClearOptions ClearMsg = new CNetMsg_Sv_VoteClearOptions();
                    Server.SendPackMsg(ClearMsg, (int)Consts.MSGFLAG_VITAL, ClientID);
                    
                    // send tuning parameters to client
                    SendTuningParams(ClientID);

                    // client is ready to enter
                    pPlayer.m_IsReady = true;
                    CNetMsg_Sv_ReadyToEnter m = new CNetMsg_Sv_ReadyToEnter();
                    Server.SendPackMsg(m, (int)Consts.MSGFLAG_VITAL | (int)Consts.MSGFLAG_FLUSH, ClientID);
                }
            }
        }

        public override void OnClientConnected(int ClientID)
        {
            // Check which team the player should be on
            m_apPlayers[ClientID] = new CPlayer(this, ClientID, (int)Consts.TEAM_SPECTATORS);
            OnClientConnectedEvent?.Invoke(m_apPlayers[ClientID]);
        }

        private string ParseLanguageByCountry(int country)
        {
            if (country == 643 || country == 804 || country == 398)
                return "ru";
            return "en";
        }

        public void ForeachClients(Action<CPlayer> action)
        {
            for (int i = 0; i < MAX_CLIENTS; i++)
            {
                if (m_apPlayers[i] != null)
                    action(m_apPlayers[i]);
            }
        }

        public override void OnClientEnter(int ClientID)
        {
            m_apPlayers[ClientID].Respawn();
            m_apPlayers[ClientID].SetLanguage(ParseLanguageByCountry(Server.ClientCountry(ClientID)));

            ForeachClients(c =>
            {
                SendChatTarget(c.GetCID(), c.Localize("'{0}' joined to the server", Server.ClientName(ClientID)));
            });
            

            string aBuf = string.Format("team_join player='{0}:{1}' team={2}", ClientID, Server.ClientName(ClientID), m_apPlayers[ClientID].GetTeam());
            Console.Print(IConsole.OUTPUT_LEVEL_DEBUG, "game", aBuf);
            
            // send motd
            //Controller.OnClientEnter(ClientID);
        }

        public void SendMotd(int ClientID, string Message)
        {
            CNetMsg_Sv_Motd Msg = new CNetMsg_Sv_Motd { m_pMessage = Message };
            Server.SendPackMsg(Msg, (int)Consts.MSGFLAG_VITAL, ClientID);
        }

        public override void OnClientDrop(int ClientID, string pReason)
        {
            AbortVoteKickOnDisconnect(ClientID);
            m_apPlayers[ClientID].OnDisconnect(pReason);
            OnClientDropEvent?.Invoke(m_apPlayers[ClientID]);
            m_apPlayers[ClientID] = null;

            Controller.CheckTeamBalance();

            // update spectator modes
            ForeachClients(c =>
            {
                if (c.m_SpectatorID == ClientID)
                    c.m_SpectatorID = (int)Consts.SPEC_FREEVIEW;
            });
        }

        public override void OnClientDirectInput(int ClientID, int[] pInput)
        {
            CNetObj_PlayerInput input = new CNetObj_PlayerInput();
            input.Read(pInput);

            if (!m_World.m_Paused)
                m_apPlayers[ClientID].OnDirectInput(input);
        }

        public override void OnClientPredictedInput(int ClientID, int[] pInput)
        {
            CNetObj_PlayerInput input = new CNetObj_PlayerInput();
            input.Read(pInput);
            if (!m_World.m_Paused)
                m_apPlayers[ClientID].OnPredictedInput(input);
        }

        public override bool IsClientReady(int ClientID)
        {
            return m_apPlayers[ClientID] != null && m_apPlayers[ClientID].m_IsReady;
        }

        public override bool IsClientPlayer(int ClientID)
        {
            return m_apPlayers[ClientID] != null && m_apPlayers[ClientID].GetTeam() != (int)Consts.TEAM_SPECTATORS;
        }

        public override string GameType()
        {
            return !string.IsNullOrEmpty(m_pController?.m_pGameType) ? Controller.m_pGameType : "";
        }

        public override string Version()
        {
            return CVersion.GAME_VERSION;
        }

        public override string NetVersion()
        {
            return CVersion.GAME_NETVERSION;
        }

        public static IGameServer CreateGameServer()
        {
            return new CGameContext();
        }

        public static int CmaskAll()
        {
            return -1;
        }

        public static int CmaskOne(int ClientID)
        {
            return 1 << ClientID;
        }

        public static int CmaskAllExceptOne(int ClientID)
        {
            return 0x7fffffff ^ CmaskOne(ClientID);
        }

        public static bool CmaskIsSet(int Mask, int ClientID)
        {
            return (Mask & CmaskOne(ClientID)) != 0;
        }

        public void SwapTeams()
        {
            /*if (!Controller.IsTeamplay())
                return;

            SendChat(-1, CHAT_ALL, "Teams were swapped");

            for (int i = 0; i < MAX_CLIENTS; ++i)
            {
                if (m_apPlayers[i] != null && m_apPlayers[i].GetTeam() != (int)Consts.TEAM_SPECTATORS)
                    m_apPlayers[i].SetTeam(m_apPlayers[i].GetTeam() ^ 1, false);
            }

            Controller.CheckTeamBalance();*/
        }
    }
}

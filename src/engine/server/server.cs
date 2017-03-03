using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Teecsharp
{
    public class CServer : IServer
    {
        public const int
            MAX_RCONCMD_SEND = 16;

        public const int
            MSGFLAG_VITAL = (int)Consts.MSGFLAG_VITAL,
            MSGFLAG_FLUSH = (int)Consts.MSGFLAG_FLUSH,
            MSGFLAG_NOSEND = (int)Consts.MSGFLAG_NOSEND,
            CFGFLAG_SERVER = CConfiguration.CFGFLAG_SERVER,
            CFGFLAG_CLIENT = CConfiguration.CFGFLAG_CLIENT,
            CFGFLAG_STORE = CConfiguration.CFGFLAG_STORE,
            CFGFLAG_ECON = CConfiguration.CFGFLAG_ECON;


        public IGameServer GameServer
        {
            get { return m_pGameServer; }
        }

        public CConsole Console
        {
            get { return (CConsole) m_pConsole; }
        }

        public IStorage Storage
        {
            get { return m_pStorage; }
        }

        public CClient[] m_aClients;
        public IEngineMap m_pMap;

        private IGameServer m_pGameServer;
        private IConsole m_pConsole;
        private IStorage m_pStorage;
        private readonly CSnapIDPool m_IDPool;
        private readonly CSnapshotDelta m_SnapshotDelta;
        private readonly CSnapshotBuilder m_SnapshotBuilder;
        private readonly CRegister m_Register;
        private readonly CNetServer m_NetServer;
        private bool m_MapReload;
        private bool m_RunServer;
        private long m_GameStartTime;

        private string m_aCurrentMap;
        private uint m_CurrentMapCrc;
        private byte[] m_pCurrentMapData;
        private long m_CurrentMapSize;

        public override int MaxClients()
        {
            return m_NetServer.MaxClients();
        }

        public override string ClientName(int ClientID)
        {
            if (ClientID < 0 || ClientID >= MAX_CLIENTS || m_aClients[ClientID].State == CClient.STATE_EMPTY)
                return "(invalid)";
            if (m_aClients[ClientID].State == CClient.STATE_INGAME)
                return m_aClients[ClientID].m_aName;
            return "(connecting)";
        }

        public override string ClientClan(int ClientID)
        {
            if (ClientID < 0 || ClientID >= MAX_CLIENTS || m_aClients[ClientID].State == CClient.STATE_EMPTY)
                return "";
            if (m_aClients[ClientID].State == CClient.STATE_INGAME)
                return m_aClients[ClientID].m_aClan;
            return "";
        }

        public override int ClientCountry(int ClientID)
        {
            if (ClientID < 0 || ClientID >= MAX_CLIENTS || m_aClients[ClientID].State == CClient.STATE_EMPTY)
                return -1;
            if (m_aClients[ClientID].State == CClient.STATE_INGAME)
                return m_aClients[ClientID].m_Country;
            return -1;
        }

        public override bool ClientIngame(int ClientID)
        {
            return ClientID >= 0 && ClientID < MAX_CLIENTS && m_aClients[ClientID].State == CClient.STATE_INGAME;
        }

        public override bool GetClientInfo(int ClientID, out CClientInfo pInfo)
        {
            if (m_aClients[ClientID].State == CClient.STATE_INGAME)
            {
                pInfo = new CClientInfo();
                pInfo.m_pName = m_aClients[ClientID].m_aName;
                pInfo.m_Latency = m_aClients[ClientID].m_Latency;
                CGameContext gameServer = (CGameContext)m_pGameServer;
                if (gameServer.m_apPlayers[ClientID] != null)
                    pInfo.m_ClientVersion = gameServer.m_apPlayers[ClientID].m_ClientVersion;
                return true;
            }
            pInfo = default(CClientInfo);
            return false;
        }

        public override bool GetClientAddr(int ClientID, out string pAddrStr, int Size = 48)
        {
            if (ClientID >= 0 && ClientID < MAX_CLIENTS && m_aClients[ClientID].State == CClient.STATE_INGAME)
            {
                pAddrStr = CSystem.net_addr_str(m_NetServer.ClientAddr(ClientID), Size, false);
                return true;
            }
            pAddrStr = "";
            return false;
        }

        private bool TrySetClientName(int ClientID, string pName)
        {
            StringBuilder aTrimmedName = new StringBuilder(pName.Trim());

            // check for empty names
            if (string.IsNullOrEmpty(aTrimmedName.ToString()))
                return true;

            // check if new and old name are the same
            if (!string.IsNullOrEmpty(m_aClients[ClientID].m_aName) && m_aClients[ClientID].m_aName == aTrimmedName.ToString())
                return false;

            string aBuf = string.Format("'{0}' . '{1}'", pName, aTrimmedName);
            Console.Print(IConsole.OUTPUT_LEVEL_ADDINFO, "server", aBuf);
            pName = aTrimmedName.ToString();

            // make sure that two clients doesn't have the same name
            for (int i = 0; i < MAX_CLIENTS; i++)
            {
                if (i != ClientID && m_aClients[i].State >= CClient.STATE_READY)
                {
                    if (pName == m_aClients[i].m_aName)
                        return true;
                }
            }

            // set the client name
            m_aClients[ClientID].m_aName = pName;
            return false;
        }

        public override void SetClientName(int ClientID, string pName)
        {
            if (ClientID < 0 || ClientID >= MAX_CLIENTS || m_aClients[ClientID].State < CClient.STATE_READY)
                return;

            if (string.IsNullOrEmpty(pName))
                return;

            StringBuilder aCleanName = new StringBuilder(pName);
            // clear name
            for (int i = 0; i < aCleanName.Length; i++)
            {
                if (aCleanName[i] < 32)
                    aCleanName[i] = ' ';
            }

            if (TrySetClientName(ClientID, aCleanName.ToString()))
            {
                // auto rename
                for (int i = 1; ; i++)
                {
                    string aNameTry = string.Format("({0}){1}", i, aCleanName);
                    if (!TrySetClientName(ClientID, aNameTry))
                        break;
                }
            }
        }

        public override void SetClientClan(int ClientID, string pClan)
        {
            if (ClientID < 0 || ClientID >= MAX_CLIENTS || m_aClients[ClientID].State < CClient.STATE_READY || string.IsNullOrEmpty(pClan))
                return;
            m_aClients[ClientID].m_aClan = pClan;
        }

        public override void SetClientCountry(int ClientID, int Country)
        {
            if (ClientID < 0 || ClientID >= MAX_CLIENTS || m_aClients[ClientID].State < CClient.STATE_READY)
                return;
            m_aClients[ClientID].m_Country = Country;
        }

        public override void SetClientScore(int ClientID, int Score)
        {
            if (ClientID < 0 || ClientID >= MAX_CLIENTS || m_aClients[ClientID].State < CClient.STATE_READY)
                return;
            m_aClients[ClientID].m_Score = Score;
        }

        public override int SnapNewID()
        {
            return m_IDPool.NewID();
        }

        public override void SnapFreeID(int ID)
        {
            m_IDPool.FreeID(ID);
        }

        public override object SnapEvent(Type Type, int TypeID, int ID)
        {
            if (TypeID < 0 && TypeID > 65535)
                CSystem.dbg_msg("[server]", "incorrect type");
            if (ID < 0 && ID > 65535)
                CSystem.dbg_msg("[server]", "incorrect id");

            return ID < 0 ? null : m_SnapshotBuilder.NewEvent(Type, ID, TypeID);
        }

        public override T SnapNetObj<T>(int Type, int ID)
        {
            if (Type < 0 && Type > 65535)
                CSystem.dbg_msg("[server]", "incorrect type");
            if (ID < 0 && ID > 65535)
                CSystem.dbg_msg("[server]", "incorrect id");

            return ID < 0 ? null : m_SnapshotBuilder.NewNetObj<T>(ID, Type);
        }

        public override bool IsAuthed(int ClientID)
        {
            return m_aClients[ClientID].m_Authed != 0;
        }

        public override void Kick(int ClientID, string pReason, int from)
        {
            if (ClientID < 0 || ClientID >= MAX_CLIENTS || m_aClients[ClientID].State == CClient.STATE_EMPTY)
            {
                Console.Print(IConsole.OUTPUT_LEVEL_STANDARD, "server", "invalid client id to kick");
                return;
            }
            if (from == ClientID)
            {
                Console.Print(IConsole.OUTPUT_LEVEL_STANDARD, "server", "you can't kick yourself");
                return;
            }
            if (m_aClients[ClientID].m_Authed < IConsole.ACCESS_LEVEL_NO)
            {
                Console.Print(IConsole.OUTPUT_LEVEL_STANDARD, "server", "kick command denied");
                return;
            }

            m_NetServer.Drop(ClientID, pReason);
        }

        public override int GetIdMap(int ClientID)
        {
            return VANILLA_MAX_CLIENTS * ClientID;
        }

        public override void SetCustClt(int ClientID)
        {
            m_aClients[ClientID].m_CustClt = true;
        }

        public override bool GetClientAddr(int ClientID, out NETADDR pAddr)
        {
            if (ClientID >= 0 && ClientID < MAX_CLIENTS && m_aClients[ClientID].State == CClient.STATE_INGAME)
            {
                pAddr = m_NetServer.ClientAddr(ClientID);
                return true;
            }
            pAddr = null;
            return false;
        }

        private long TickStartTime(int Tick)
        {
            return m_GameStartTime + (CSystem.time_freq() * Tick) / (int)Consts.SERVER_TICK_SPEED;
        }

        private void UpdateClientRconCommands()
        {
            int ClientID = Tick() % MAX_CLIENTS;
            if (m_aClients[ClientID].State != CClient.STATE_EMPTY &&
                m_aClients[ClientID].m_Authed != IConsole.ACCESS_LEVEL_NO &&
                m_aClients[ClientID].m_pRconCmdToSend != null)
            {
                var enumerator = m_aClients[ClientID].m_pRconCmdToSend;
                CSystem.dbg_msg_clr("test", "start send rcons commands", ConsoleColor.Red);
                bool sended = false;
                for (int i = 0; i < MAX_RCONCMD_SEND && enumerator != null && enumerator.MoveNext(); ++i)
                {
                    sended = true;
                    SendRconCmdAdd(enumerator.Current.Value, ClientID);
                }

                if (!sended)
                {
                    m_aClients[ClientID].m_pRconCmdToSend.Dispose();
                    m_aClients[ClientID].m_pRconCmdToSend = null;
                }
            }
        }

        private void DoSnapshot()
        {
            GameServer.OnPreSnap();

            // create snapshots for all clients
            for (int i = 0; i < MAX_CLIENTS; i++)
            {
                // client must be ingame to recive snapshots
                if (m_aClients[i].State != CClient.STATE_INGAME)
                    continue;

                // this client is trying to recover, don't spam snapshots
                if (m_aClients[i].m_SnapRate == CClient.SNAPRATE_RECOVER && (Tick()%50) != 0)
                    continue;

                // this client is trying to recover, don't spam snapshots
                if (m_aClients[i].m_SnapRate == CClient.SNAPRATE_INIT && (Tick()%10) != 0)
                    continue;

                m_SnapshotBuilder.Init();
                GameServer.OnSnap(i);

                // finish snapshot
                var pCurrentSnapshot = m_SnapshotBuilder.Finish();
                int Crc = pCurrentSnapshot.Crc();

                // remove old snapshos
                // keep 3 seconds worth of snapshots
                m_aClients[i].m_Snapshots.PurgeUntil(m_CurrentGameTick - (int)Consts.SERVER_TICK_SPEED * 3);

                // save it the snapshot
                m_aClients[i].m_Snapshots.Add(m_CurrentGameTick, CSystem.time_get(), pCurrentSnapshot.DataSize(), pCurrentSnapshot);

                var pDeltashot = new CSnapshot();
                long tagTime = 0;
                int DeltaTick = -1;
                int DeltaSize = 0;

                var DeltashotSize = m_aClients[i].m_Snapshots.Get(m_aClients[i].m_LastAckedSnapshot, ref tagTime, ref pDeltashot);
                if (DeltashotSize >= 0)
                {
                    DeltaTick = m_aClients[i].m_LastAckedSnapshot;
                }
                else
                {
                    // no acked package found, force client to recover rate
                    if (m_aClients[i].m_SnapRate == CClient.SNAPRATE_FULL)
                        m_aClients[i].m_SnapRate = CClient.SNAPRATE_RECOVER;
                }
                //CSystem.dbg_msg("test", "DeltashotSize {0}", DeltashotSize);

                int[] aDeltaData = new int[CSnapshotBuilder.MAX_SIZE/sizeof(int)];
                // create delta
                DeltaSize = m_SnapshotDelta.CreateDelta(pDeltashot, pCurrentSnapshot, aDeltaData) * sizeof(int);

                //int currentGameTickOffset = g_Config.GetInt("CurrentGameTick");
                if (DeltaSize != 0)
                {
                    // compress it
                    const int MaxSize = (int)Consts.MAX_SNAPSHOT_PACKSIZE;

                    byte[] result = new byte[aDeltaData.Length * sizeof(int)];
                    Buffer.BlockCopy(aDeltaData, 0, result, 0, result.Length);
                    byte[] aCompData = new byte[DeltaSize*2];

                    var SnapshotSize = (int)CVariableInt.Compress(result, 0, DeltaSize, aCompData, 0);
                    var NumPackets = (SnapshotSize + MaxSize - 1) / MaxSize;

                    for (int n = 0, Left = SnapshotSize; Left != 0; n++)
                    {
                        int Chunk = Left < MaxSize ? Left : MaxSize;
                        Left -= Chunk;

                        if (NumPackets == 1)
                        {
                            CMsgPacker Msg = new CMsgPacker((int)NetworkConsts.NETMSG_SNAPSINGLE);
                            Msg.AddInt(m_CurrentGameTick);
                            Msg.AddInt(m_CurrentGameTick - DeltaTick);
                            Msg.AddInt(Crc);
                            Msg.AddInt(Chunk);
                            Msg.AddRaw(aCompData, n * MaxSize, Chunk);
                            SendMsgEx(Msg, MSGFLAG_FLUSH, i, true);
                            //CSystem.dbg_msg("test", "1 {0}", DeltaTick);
                        }
                        else
                        {
                            CMsgPacker Msg = new CMsgPacker((int)NetworkConsts.NETMSG_SNAP);
                            Msg.AddInt(m_CurrentGameTick);
                            Msg.AddInt(m_CurrentGameTick - DeltaTick);
                            Msg.AddInt(NumPackets);
                            Msg.AddInt(n);
                            Msg.AddInt(Crc);
                            Msg.AddInt(Chunk);
                            Msg.AddRaw(aCompData, n * MaxSize, Chunk);
                            SendMsgEx(Msg, MSGFLAG_FLUSH, i, true);
                            //CSystem.dbg_msg("test", "2 {0}", DeltaTick);
                        }
                    }
                }
                else
                {
                    CMsgPacker Msg = new CMsgPacker((int)NetworkConsts.NETMSG_SNAPEMPTY);
                    Msg.AddInt(m_CurrentGameTick);
                    Msg.AddInt(m_CurrentGameTick - DeltaTick);
                    SendMsgEx(Msg, MSGFLAG_FLUSH, i, true);
                    //CSystem.dbg_msg("test", "3 {0}", DeltaTick);
                }
            }

            GameServer.OnPostSnap();
        }

        public override bool SendMsg(CMsgPacker pMsg, int Flags, int ClientID)
        {
            return SendMsgEx(pMsg, Flags, ClientID, false);
        }

        public bool SendMsgEx(CMsgPacker pMsg, int Flags, int ClientID, bool System)
        {
            CNetChunk Packet = new CNetChunk();

            if (pMsg == null)
                return false;

            Packet.m_ClientID = ClientID;
            Packet.m_pData = pMsg.Data();
            Packet.m_DataSize = pMsg.Size();

            Packet.m_pData[0] = (byte) (Packet.m_pData[0] << 1);
            if (System)
                Packet.m_pData[0] = (byte) (Packet.m_pData[0] | 1);

            if ((Flags & MSGFLAG_VITAL) != 0)
                Packet.m_Flags |= (int)NetworkConsts.NETSENDFLAG_VITAL;
            if ((Flags & MSGFLAG_FLUSH) != 0)
                Packet.m_Flags |= (int)NetworkConsts.NETSENDFLAG_FLUSH;

            if ((Flags & MSGFLAG_NOSEND) == 0)
            {
                if (ClientID == -1)
                {
                    // broadcast
                    for (int i = 0; i < MAX_CLIENTS; i++)
                    {
                        if (m_aClients[i].State == CClient.STATE_INGAME)
                        {
                            Packet.m_ClientID = i;
                            m_NetServer.Send(Packet);
                        }
                    }
                }
                else
                    m_NetServer.Send(Packet);
            }
            return true;
        }

        public void SendRconLine(int ClientID, string pLine)
        {
            CMsgPacker Msg = new CMsgPacker((int)NetworkConsts.NETMSG_RCON_LINE);
            Msg.AddString(pLine, 512);
            SendMsgEx(Msg, MSGFLAG_VITAL, ClientID, true);
        }

        static int ReentryGuard = 0;
        private void SendRconLineAuthed(string pStr, object pUser)
        {
	        CServer pThis = (CServer)pUser;

	        if(ReentryGuard != 0)
                return;
	        ReentryGuard++;

	        for(int i = 0; i < MAX_CLIENTS; i++)
	        {
		        if(pThis.m_aClients[i].State != CClient.STATE_EMPTY && pThis.m_aClients[i].m_Authed < IConsole.ACCESS_LEVEL_NO)
			        pThis.SendRconLine(i, pStr);
	        }

	        ReentryGuard--;
        }

        private void SendRconCmdAdd(CConsoleCommand command, int ClientID)
        {
	        CMsgPacker Msg = new CMsgPacker((int)NetworkConsts.NETMSG_RCON_CMD_ADD);
	        Msg.AddString(command.Name, IConsole.TEMPCMD_NAME_LENGTH);
	        Msg.AddString(command.Help, IConsole.TEMPCMD_HELP_LENGTH);
	        Msg.AddString(command.Format, IConsole.TEMPCMD_PARAMS_LENGTH);
	        SendMsgEx(Msg, MSGFLAG_VITAL, ClientID, true);
        }

        private void SendRconCmdRem(CConsoleCommand command, int ClientID)
        {
	        CMsgPacker Msg = new CMsgPacker((int)NetworkConsts.NETMSG_RCON_CMD_REM);
	        Msg.AddString(command.Name, 256);
	        SendMsgEx(Msg, MSGFLAG_VITAL, ClientID, true);
        }

        static readonly int[] lastsent = new int[MAX_CLIENTS];
        static readonly int[] lastask = new int[MAX_CLIENTS];
        static readonly int[] lastasktick = new int[MAX_CLIENTS];

        public string GetMapName()
        {
            // get the name of the map without his path
            string pMapShortName = g_Config.GetString("SvMap");

            for (int i = 0; i < (g_Config.GetString("SvMap")).Length - 1; i++)
            {
                if ((g_Config.GetString("SvMap"))[i] == '/' || (g_Config.GetString("SvMap"))[i] == '\\')
                    pMapShortName = (g_Config.GetString("SvMap")).Substring(i + 1);
            }
            return pMapShortName;
        }

        void SendMap(int ClientID)
        {
            lastsent[ClientID] = 0;
            lastask[ClientID] = 0;
            lastasktick[ClientID] = Tick();

            CMsgPacker Msg = new CMsgPacker((int)NetworkConsts.NETMSG_MAP_CHANGE);
            Msg.AddString(GetMapName());
            Msg.AddInt((int)m_CurrentMapCrc);
            Msg.AddInt((int)m_CurrentMapSize);
            SendMsgEx(Msg, MSGFLAG_VITAL | MSGFLAG_FLUSH, ClientID, true);
        }

        void SendConnectionReady(int ClientID)
        {
            CMsgPacker Msg = new CMsgPacker((int)NetworkConsts.NETMSG_CON_READY);
            SendMsgEx(Msg, MSGFLAG_VITAL | MSGFLAG_FLUSH, ClientID, true);
        }

        void BanAdd(NETADDR Addr, int Seconds, string pReason)
        {
        }

        void ProcessClientPacket(CNetChunk pPacket)
        {
            int ClientID = pPacket.m_ClientID;
            CUnpacker Unpacker = new CUnpacker();
            Unpacker.Reset(pPacket.m_pData, pPacket.m_DataSize);

            // unpack msgid and system flag
            int Msg = Unpacker.GetInt();
            int Sys = Msg & 1;
            Msg >>= 1;

            if (Unpacker.Error())
                return;
           
            if (g_Config.GetInt("SvNetlimit") != 0 && Msg != (int)NetworkConsts.NETMSG_REQUEST_MAP_DATA)
	        {
		        long Now = CSystem.time_get();
                long Diff = Now - m_aClients[ClientID].m_TrafficSince;
		        float Alpha = g_Config.GetInt("SvNetlimitAlpha") / 100.0f;
		        float Limit = (float)g_Config.GetInt("SvNetlimit") * 1024 / CSystem.time_freq();

		        if (m_aClients[ClientID].m_Traffic > Limit)
		        {
			        m_NetServer.NetBan().BanAddr(pPacket.m_Address, 600, "Stressing network");
			        return;
		        }
		        if (Diff > 100)
		        {
			        m_aClients[ClientID].m_Traffic = (Alpha * ((float)pPacket.m_DataSize / Diff)) + (1.0f - Alpha) * m_aClients[ClientID].m_Traffic;
			        m_aClients[ClientID].m_TrafficSince = Now;
		        }
	        }
            

            if (Sys != 0)
            {
                // system message
                if (Msg == (int)NetworkConsts.NETMSG_INFO)
                {
                    if (m_aClients[ClientID].State == CClient.STATE_AUTH)
                    {
                        string pVersion = Unpacker.GetString(CUnpacker.SANITIZE_CC);
                        CSystem.dbg_msg("msg", "Client {0} pVersion: {1}", ClientID, pVersion);

                        //if (pVersion != GameServer.NetVersion())
                        //{
                        // wrong version
                        //}

                        string pPassword = Unpacker.GetString(CUnpacker.SANITIZE_CC);
                        CSystem.dbg_msg("msg", "Client {0} pPassword: {1}", ClientID, pPassword);
                        var serverPass = g_Config.GetString("Password");
                        if (!string.IsNullOrEmpty(serverPass) && serverPass != pPassword)
                        {
                            //wrong password
                            m_NetServer.Drop(ClientID, "Wrong password");
                            return;
                        }

                        // reserved slot
                        if (ClientID >= g_Config.GetInt("SvMaxClients") - g_Config.GetInt("SvReservedSlots") && 
                            !string.IsNullOrEmpty(g_Config.GetString("SvReservedSlotsPass")) && g_Config.GetString("SvReservedSlotsPass") != pPassword)
                        {
                            m_NetServer.Drop(ClientID, "This server is full");
                            return;
                        }

                        m_aClients[ClientID].SetState(CClient.STATE_CONNECTING);
                        SendMap(ClientID);
                    }
                }
                else if (Msg == (int)NetworkConsts.NETMSG_REQUEST_MAP_DATA)
                {
                    if (m_aClients[ClientID].State < CClient.STATE_CONNECTING)
                        return; // no map w/o password, sorry guys


                    int Chunk = Unpacker.GetInt();
                    int ChunkSize = 1024 - 128;
                    int Offset = Chunk * ChunkSize;
                    int Last = 0;

                    lastask[ClientID] = Chunk;
                    lastasktick[ClientID] = Tick();
                    if (Chunk == 0)
                    {
                        lastsent[ClientID] = 0;
                    }

                    // drop faulty map data requests
                    if (Chunk < 0 || Offset > m_CurrentMapSize)
                        return;

                    if (Offset + ChunkSize >= m_CurrentMapSize)
                    {
                        ChunkSize = (int)(m_CurrentMapSize - Offset);
                        if (ChunkSize < 0)
                            ChunkSize = 0;
                        Last = 1;
                    }

                    if (lastsent[ClientID] < Chunk + g_Config.GetInt("SvMapWindow") && g_Config.GetInt("SvFastDownload") != 0)
                        return;

                    CMsgPacker MsgPacker = new CMsgPacker((int)NetworkConsts.NETMSG_MAP_DATA);
                    MsgPacker.AddInt(Last);
                    MsgPacker.AddInt((int)m_CurrentMapCrc);
                    MsgPacker.AddInt(Chunk);
                    MsgPacker.AddInt(ChunkSize);
                    MsgPacker.AddRaw(m_pCurrentMapData, Offset, ChunkSize);
                    SendMsgEx(MsgPacker, MSGFLAG_FLUSH, ClientID, true);

                    /*
                    if (g_Config["Debug"] != 0)
                    {
                        CSystem.dbg_msg("server", "[1] sending chunk {0} with size {1}", Chunk, ChunkSize);
                    }
                    */
                }
                else if (Msg == (int)NetworkConsts.NETMSG_READY)
                {
                    if (m_aClients[ClientID].State == CClient.STATE_CONNECTING)
                    {
                        string aBuf = string.Format("player is ready. ClientID={0} addr={1}",
                            ClientID, m_NetServer.ClientAddr(ClientID).IpStr);

                        CSystem.dbg_msg("server", aBuf);
                        m_aClients[ClientID].SetState(CClient.STATE_READY);

                        GameServer.OnClientConnected(ClientID);
                        SendConnectionReady(ClientID);
                    }
                }
                else if (Msg == (int)NetworkConsts.NETMSG_ENTERGAME)
                {
                    if (m_aClients[ClientID].State == CClient.STATE_READY &&
                        GameServer.IsClientReady(ClientID))
                    {
                        string aBuf = string.Format("player has entered the game. ClientID={0} addr={1}",
                            ClientID, m_NetServer.ClientAddr(ClientID).IpStr);
                        CSystem.dbg_msg("server", aBuf);
                        m_aClients[ClientID].SetState(CClient.STATE_INGAME);
                        GameServer.OnClientEnter(ClientID);
                    }
                }
                else if (Msg == (int)NetworkConsts.NETMSG_INPUT)
                {
                    m_aClients[ClientID].m_LastAckedSnapshot = Unpacker.GetInt();
                    int IntendedTick = Unpacker.GetInt();
                    int Size = Unpacker.GetInt();

                    // check for errors
                    if (Unpacker.Error() || Size / 4 > (int)Consts.MAX_INPUT_SIZE)
                        return;

                    if (m_aClients[ClientID].m_LastAckedSnapshot > 0)
                        m_aClients[ClientID].m_SnapRate = CClient.SNAPRATE_FULL;

                    long TagTime = 0;
                    CSnapshot outPtr1 = null;
                    if (m_aClients[ClientID].m_Snapshots.Get(m_aClients[ClientID].m_LastAckedSnapshot, ref TagTime, ref outPtr1) >= 0)
                        m_aClients[ClientID].m_Latency = (int)(((CSystem.time_get() - TagTime) * 1000) / CSystem.time_freq());

                    // add message to report the input timing
                    // skip packets that are old
                    if (IntendedTick > m_aClients[ClientID].m_LastInputTick)
                    {
                        int TimeLeft = (int)(((TickStartTime(IntendedTick) - CSystem.time_get()) * 1000) / CSystem.time_freq());

                        CMsgPacker MsgPacker = new CMsgPacker((int)NetworkConsts.NETMSG_INPUTTIMING);
                        MsgPacker.AddInt(IntendedTick);
                        MsgPacker.AddInt(TimeLeft);
                        SendMsgEx(MsgPacker, 0, ClientID, true);
                    }

                    m_aClients[ClientID].m_LastInputTick = IntendedTick;

                    var pInput = m_aClients[ClientID].m_aInputs[m_aClients[ClientID].m_CurrentInput];

                    if (IntendedTick <= Tick())
                        IntendedTick = Tick() + 1;

                    pInput.m_GameTick = IntendedTick;

                    for (int i = 0; i < Size / 4; i++)
                        pInput.m_aData[i] = Unpacker.GetInt();

                    Array.Copy(pInput.m_aData, 0, m_aClients[ClientID].m_LatestInput.m_aData, 0, (int)Consts.MAX_INPUT_SIZE);

                    m_aClients[ClientID].m_CurrentInput++;
                    m_aClients[ClientID].m_CurrentInput %= 200;

                    // call the mod with the fresh input data
                    if (m_aClients[ClientID].State == CClient.STATE_INGAME)
                        GameServer.OnClientDirectInput(ClientID, m_aClients[ClientID].m_LatestInput.m_aData);
                }
                else if (Msg == (int)NetworkConsts.NETMSG_RCON_CMD)
                {
                    string pCmd = Unpacker.GetString();
                    if (Unpacker.Error() || m_aClients[ClientID].m_Authed == IConsole.ACCESS_LEVEL_NO)
                        return;

                    string aBuf = string.Format("ClientID={0} rcon='{1}'", ClientID, pCmd);
                    Console.Print(IConsole.OUTPUT_LEVEL_ADDINFO, "server", aBuf);
                    Console.ExecuteLine(pCmd, m_aClients[ClientID].m_Authed);
                }
                else if (Msg == (int)NetworkConsts.NETMSG_RCON_AUTH)
                {
                    Unpacker.GetString(); // login name, not used
                    var pPw = Unpacker.GetString(CUnpacker.SANITIZE_CC);

                    if (Unpacker.Error())
                        return;

                    string SvRconPassword = g_Config.GetString("SvRconPassword");
                    string SvRconModPassword = g_Config.GetString("SvRconModPassword");

                    if (string.IsNullOrEmpty(SvRconPassword) &&
                        string.IsNullOrEmpty(SvRconModPassword))
                    {
                        SendRconLine(ClientID, "No rcon password set on server. Set sv_rcon_password and/or sv_rcon_mod_password to enable the remote console.");
                    }
                    else if (!string.IsNullOrEmpty(SvRconPassword) && pPw == SvRconPassword)
                    {
                        CMsgPacker newMsg = new CMsgPacker((int)NetworkConsts.NETMSG_RCON_AUTH_STATUS);
                        newMsg.AddInt(1);  //authed
                        newMsg.AddInt(1);  //cmdlist
                        SendMsgEx(newMsg, MSGFLAG_VITAL, ClientID, true);

                        m_aClients[ClientID].m_Authed = IConsole.ACCESS_LEVEL_ADMIN;
                        int SendRconCmds = Unpacker.GetInt();
                        if (!Unpacker.Error() && SendRconCmds != 0)
                            m_aClients[ClientID].m_pRconCmdToSend = Console.GetEnumerator(IConsole.ACCESS_LEVEL_ADMIN, CFGFLAG_SERVER);
                        SendRconLine(ClientID, "Admin authentication successful. Full remote console access granted.");
                        string aBuf = string.Format("ClientID={0} authed (admin)", ClientID);
                        Console.Print(IConsole.OUTPUT_LEVEL_STANDARD, "server", aBuf);
                    }
                    else if (!string.IsNullOrEmpty(SvRconModPassword) && pPw == SvRconModPassword)
                    {
                        CMsgPacker newMsg = new CMsgPacker((int)NetworkConsts.NETMSG_RCON_AUTH_STATUS);
                        newMsg.AddInt(1);  //authed
                        newMsg.AddInt(1);  //cmdlist
                        SendMsgEx(newMsg, MSGFLAG_VITAL, ClientID, true);

                        m_aClients[ClientID].m_Authed = IConsole.ACCESS_LEVEL_MOD;
                        int SendRconCmds = Unpacker.GetInt();
                        if (!Unpacker.Error() && SendRconCmds != 0)
                            m_aClients[ClientID].m_pRconCmdToSend = Console.GetEnumerator(IConsole.ACCESS_LEVEL_MOD, CFGFLAG_SERVER);
                        SendRconLine(ClientID, "Moderator authentication successful. Limited remote console access granted.");
                        string aBuf = string.Format("ClientID={0} authed (moderator)", ClientID);
                        Console.Print(IConsole.OUTPUT_LEVEL_STANDARD, "server", aBuf);
                    }
                    else if (g_Config.GetInt("SvRconMaxTries") != 0)
                    {
                        m_aClients[ClientID].m_AuthTries++;
                        string aBuf = string.Format("Wrong password {0}/{1}.", m_aClients[ClientID].m_AuthTries, g_Config.GetInt("SvRconMaxTries"));
                        SendRconLine(ClientID, aBuf);
                        if (m_aClients[ClientID].m_AuthTries >= g_Config.GetInt("SvRconMaxTries"))
                        {
                            if (g_Config.GetInt("SvRconBantime") == 0)
                                m_NetServer.Drop(ClientID, "Too many remote console authentication tries");
                            else
                            {
                                NETADDR Addr = m_NetServer.ClientAddr(ClientID);
                                BanAdd(Addr, g_Config.GetInt("SvRconBantime") * 60, "Too many remote console authentication tries");
                            }
                        }
                    }
                    else
                    {
                        SendRconLine(ClientID, "Wrong password.");
                    }
                }
                else if (Msg == (int)NetworkConsts.NETMSG_PING)
                {
                    CMsgPacker MsgPacker = new CMsgPacker((int)NetworkConsts.NETMSG_PING_REPLY);
                    SendMsgEx(MsgPacker, 0, ClientID, true);
                }
                else
                {
                    if (g_Config.GetInt("Debug") != 0)
                    {
                        string aBuf = string.Format("strange message ClientID={0} msg={1} data_size={2}",
                            ClientID, Msg, pPacket.m_DataSize);
                        Console.Print(IConsole.OUTPUT_LEVEL_DEBUG, "server", aBuf);
                    }
                }
            }
            else
            {
                // game message
                if (m_aClients[ClientID].State >= CClient.STATE_READY)
                    GameServer.OnMessage(Msg, Unpacker, ClientID);
            }
        }

        private void PumpNetwork()
        {
            CNetChunk Packet = new CNetChunk();
            m_NetServer.Update();

            while (m_NetServer.Recv(Packet))
            {
                if (Packet.m_ClientID == -1)
                {
                    // stateless
                    if (!m_Register.RegisterProcessPacket(Packet))
                    {
                        if (Packet.m_DataSize == CMasterServer.SERVERBROWSE_GETINFO.Length + 1 &&
                            CSystem.mem_comp(Packet.m_pData, CMasterServer.SERVERBROWSE_GETINFO))
                        {
                            SendServerInfo(Packet.m_Address, Packet.m_pData[CMasterServer.SERVERBROWSE_GETINFO.Length], false);
				        }
				        else if (Packet.m_DataSize == CMasterServer.SERVERBROWSE_GETINFO64.Length + 1 &&
                            CSystem.mem_comp(Packet.m_pData, CMasterServer.SERVERBROWSE_GETINFO64))
				        {
                            SendServerInfo(Packet.m_Address, Packet.m_pData[CMasterServer.SERVERBROWSE_GETINFO64.Length], true);
				        }
                    }
                }
                else
                    ProcessClientPacket(Packet);
            }

            if (g_Config.GetInt("SvFastDownload") != 0)
            {
                for (int i = 0; i < MAX_CLIENTS; i++)
                {
                    if (m_aClients[i].State != CClient.STATE_CONNECTING)
                        continue;
                    if (lastasktick[i] < Tick() - TickSpeed())
                    {
                        lastsent[i] = lastask[i];
                        lastasktick[i] = Tick();
                    }
                    if (lastask[i] < lastsent[i] - g_Config.GetInt("SvMapWindow"))
                        continue;

                    int Chunk = lastsent[i]++;
                    int ChunkSize = 1024 - 128;
                    int Offset = Chunk * ChunkSize;
                    int Last = 0;

                    // drop faulty map data requests
                    if (Chunk < 0 || Offset > m_CurrentMapSize)
                        continue;
                    if (Offset + ChunkSize >= m_CurrentMapSize)
                    {
                        ChunkSize = (int)(m_CurrentMapSize - Offset);
                        if (ChunkSize < 0)
                            ChunkSize = 0;
                        Last = 1;
                    }

                    CMsgPacker MsgPacker = new CMsgPacker((int)NetworkConsts.NETMSG_MAP_DATA);
                    MsgPacker.AddInt(Last);
                    MsgPacker.AddInt((int)m_CurrentMapCrc);
                    MsgPacker.AddInt(Chunk);
                    MsgPacker.AddInt(ChunkSize);
                    MsgPacker.AddRaw(m_pCurrentMapData, Offset, ChunkSize);
                    SendMsgEx(MsgPacker, MSGFLAG_FLUSH, i, true);

                    if (g_Config.GetInt("Debug") != 0)
                    {
                        //CSystem.dbg_msg("server", "sending chunk {0} with size {1}", Chunk, ChunkSize);
                    }
                }
            }

            //m_ServerBan.Update();
            //m_Econ.Update();
        }

        private bool LoadMap(string pMapName)
        {
            string aBuf = string.Format("maps/{0}.map", pMapName);

            // check for valid standard map
            //if(!m_MapChecker.ReadAndValidateMap(Storage(), aBuf, IStorage::TYPE_ALL))
            //{
            //    Console.Print(IConsole.OUTPUT_LEVEL_STANDARD, "mapchecker", "invalid standard map");
            //    return 0;
            //}

            if (!m_pMap.Load(aBuf))
                return false;

            // stop recording when we change map
            //m_DemoRecorder.Stop();

            // reinit snapshot ids
            m_IDPool.TimeoutIDs();

            // get the crc of the map
            m_CurrentMapCrc = m_pMap.Crc();

            CSystem.dbg_msg("server", "{0} crc is {1}", aBuf, m_CurrentMapCrc);
            m_aCurrentMap = pMapName;

            // load complete map into memory for download
            FileStream File = m_pMap.GetStream();
            CSystem.io_seek(File, 0, SeekOrigin.Begin);
            m_CurrentMapSize = CSystem.io_length(File);
            byte[] mapData = new byte[m_CurrentMapSize];
            CSystem.io_read(File, mapData, (int)m_CurrentMapSize);
            m_pCurrentMapData = mapData;

            return true;
        }

        private void Run()
        {
            m_pGameServer = Kernel.RequestInterface<IGameServer>();
            m_pMap = Kernel.RequestInterface<IEngineMap>();
            m_pStorage = Kernel.RequestInterface<IStorage>();

            Console.RegisterPrintCallback(g_Config.GetInt("ConsoleOutputLevel"), SendRconLineAuthed, this);

            // load map
            if (!LoadMap(g_Config.GetString("SvMap")))
            {
                CSystem.dbg_msg("server", "failed to load map. mapname='{0}'", g_Config.GetString("SvMap"));
                return;
            }

            // start server
            var localIp = string.IsNullOrEmpty(g_Config.GetString("Bindaddr")) ? CSystem.get_local_ip_address() : g_Config.GetString("Bindaddr");
            var BindAddr = CSystem.net_addr_from_str(localIp + ":" + g_Config.GetInt("SvPort"));
            BindAddr.type = (int)NetworkConsts.NETTYPE_IPV4;

            if (!m_NetServer.Open(BindAddr, null, g_Config.GetInt("SvMaxClients"), g_Config.GetInt("SvMaxClientsPerIP"), 0))
            {
                CSystem.dbg_msg("server", "couldn't open socket. port {0} might already be in use", g_Config.GetInt("SvPort"));
                return;
            }

            m_NetServer.SetCallbacks(NewClientCallback, DelClientCallback);

            string aBuf = string.Format("server name is '{0}'", g_Config.GetString("SvName"));
            Console.Print(IConsole.OUTPUT_LEVEL_STANDARD, "server", aBuf);

            GameServer.OnInit();
            aBuf = string.Format("version {0}", GameServer.NetVersion());
            Console.Print(IConsole.OUTPUT_LEVEL_STANDARD, "server", aBuf);

            Thread.Sleep(1000);

            m_GameStartTime = CSystem.time_get();
            int NewTicks;
            long Now;
            int NewTicksPerSec = 0;
            int ReportTime = Tick() + TickSpeed();

            while (m_RunServer)
            {
                Now = CSystem.time_get();
                NewTicks = 0;

                // load new map TODO: don't poll this
                if (g_Config.GetString("SvMap") != m_aCurrentMap || m_MapReload)
                {
                    m_MapReload = false;

                    // load map
                    if (LoadMap(g_Config.GetString("SvMap")))
                    {
                        // new map loaded
                        GameServer.OnShutdown();

                        for (int c = 0; c < MAX_CLIENTS; c++)
                        {
                            if (m_aClients[c].State <= CClient.STATE_AUTH)
                                continue;

                            SendMap(c);
                            m_aClients[c].Reset();
                            m_aClients[c].SetState(CClient.STATE_CONNECTING);
                        }

                        m_GameStartTime = CSystem.time_get();
                        m_CurrentGameTick = 0;
                        Kernel.ReregisterInterface(GameServer);
                        GameServer.OnInit();
                        UpdateServerInfo();
                    }
                    else
                    {
                        aBuf = string.Format("failed to load map. mapname='{0}'", g_Config.GetString("SvMap"));
                        Console.Print(IConsole.OUTPUT_LEVEL_STANDARD, "server", aBuf);
                        Console.ExecuteLine("sv_map " + m_aCurrentMap, IConsole.ACCESS_LEVEL_ADMIN);
                    }
                }

                while (Now > TickStartTime(m_CurrentGameTick + 1))
                {
                    m_CurrentGameTick++;
                    NewTicks++;
                    
                    // apply new input
                    for (int c = 0; c < MAX_CLIENTS; c++)
                    {
                        if (m_aClients[c].State != CClient.STATE_INGAME)
                            continue;
                        for (int i = 0; i < 200; i++)
                        {
                            if (m_aClients[c].m_aInputs[i].m_GameTick == Tick())
                            {
                                GameServer.OnClientPredictedInput(c, m_aClients[c].m_aInputs[i].m_aData);
                                break;
                            }
                        }
                    }

                    GameServer.OnTick();
                }

                // snap game
                if (NewTicks != 0)
                {
                    if (m_CurrentGameTick % 2 == 0 || g_Config.GetInt("SvHighBandwidth") != 0)
                        DoSnapshot();
                    UpdateClientRconCommands();
                }

               // CSystem.dbg_msg_clr("test", "NewTicks {0}", ConsoleColor.Red, NewTicks);

                // master server stuff
                m_Register.RegisterUpdate(m_NetServer.NetType());
                PumpNetwork();
                
                NewTicksPerSec += NewTicks;
                if (ReportTime <= Tick())
                {
                    ReportTime = Tick() + TickSpeed()*5;
                    //CSystem.dbg_msg_clr("test", "NewTicksPerSec: {0} | {1} | {2}", ConsoleColor.Red, NewTicksPerSec, Stopwatch.Frequency, m_CurrentGameTick);
                    NewTicksPerSec = 0;
                    //GC.Collect();
                    //GC.WaitForPendingFinalizers();
                }

                Thread.Sleep(5);
            }

            // disconnect all clients on shutdown
            for (int i = 0; i < MAX_CLIENTS; ++i)
            {
                if (m_aClients[i].State != CClient.STATE_EMPTY)
                    m_NetServer.Drop(i, "Server shutdown");
                //m_Econ.Shutdown();
            }

            GameServer.OnShutdown();
            m_pMap.Unload();
        }

        void UpdateServerInfo()
        {
            for (int i = 0; i < MAX_CLIENTS; ++i)
            {
                if (m_aClients[i].State != CClient.STATE_EMPTY)
                {
                    SendServerInfo(m_NetServer.ClientAddr(i), -1, true);
                    SendServerInfo(m_NetServer.ClientAddr(i), -1, false);
                }
            }
        }

        void SendServerInfo(NETADDR pAddr, int Token, bool ShowMore, int Offset = 0)
        {
            CNetChunk Packet = new CNetChunk();
            CPacker p = new CPacker();
            string aBuf;

            // count the players
            int PlayerCount = 0, ClientCount = 0;
            for (int i = 0; i < MAX_CLIENTS; i++)
            {
                if (m_aClients[i].State != CClient.STATE_EMPTY)
                {
                    if (GameServer.IsClientPlayer(i))
                        PlayerCount++;
                    ClientCount++;
                }
            }

            p.Reset();

            if (ShowMore)
                p.AddRaw(CMasterServer.SERVERBROWSE_INFO64, 0, CMasterServer.SERVERBROWSE_INFO64.Length);
            else
                p.AddRaw(CMasterServer.SERVERBROWSE_INFO, 0, CMasterServer.SERVERBROWSE_INFO.Length);

            p.AddString(Token.ToString(), 6);
            p.AddString(GameServer.Version(), 32);

            if (ShowMore)
            {
                p.AddString(g_Config.GetString("SvName"), 256);
            }
            else
            {
                if (m_NetServer.MaxClients() <= VANILLA_MAX_CLIENTS)
                    p.AddString(g_Config.GetString("SvName"), 64);
                else
                {
                    aBuf = string.Format("{0} [{1}/{2}]", g_Config.GetString("SvName"), ClientCount, m_NetServer.MaxClients());
                    p.AddString(aBuf, 64);
                }
            }
            p.AddString(GetMapName(), 32);
            // gametype
            p.AddString(GameServer.GameType(), 16);

            // flags
            int pass = 0;
            if (!string.IsNullOrEmpty(g_Config.GetString("Password"))) // password set
                pass |= (int)Consts.SERVER_FLAG_PASSWORD;
            //str_format(aBuf, sizeof(aBuf), "%d", i);
            p.AddString(pass.ToString(), 2);

            int MaxClients = m_NetServer.MaxClients();
            if (!ShowMore)
            {
                if (ClientCount >= VANILLA_MAX_CLIENTS)
                {
                    if (ClientCount < MaxClients)
                        ClientCount = VANILLA_MAX_CLIENTS - 1;
                    else
                        ClientCount = VANILLA_MAX_CLIENTS;
                }
                if (MaxClients > VANILLA_MAX_CLIENTS) MaxClients = VANILLA_MAX_CLIENTS;
            }

            if (PlayerCount > ClientCount)
                PlayerCount = ClientCount;

            p.AddString(PlayerCount.ToString(), 3);                                 // num players
            p.AddString((MaxClients - g_Config.GetInt("SvSpectatorSlots")).ToString(), 3); // max players
            p.AddString(ClientCount.ToString(), 3);                                 // num clients
            p.AddString(MaxClients.ToString(), 3);                                  // max clients

            if (ShowMore)
                p.AddInt(Offset);

            int ClientsPerPacket = ShowMore ? 24 : VANILLA_MAX_CLIENTS;
            int Skip = Offset;
            int Take = ClientsPerPacket;

            for (int i = 0; i < MAX_CLIENTS; i++)
            {
                if (m_aClients[i].State != CClient.STATE_EMPTY)
                {
                    if (Skip-- > 0)
                        continue;
                    if (--Take < 0)
                        break;

                    p.AddString(ClientName(i), (int)Consts.MAX_NAME_LENGTH);    // client name
                    p.AddString(ClientClan(i), (int)Consts.MAX_CLAN_LENGTH);    // client clan
                    p.AddString(m_aClients[i].m_Country.ToString(), 6);         // client country
                    p.AddString(m_aClients[i].m_Score.ToString(), 6);           // client score
                    p.AddString(GameServer.IsClientPlayer(i) ? "1" : "0", 2); // is player?
                }
            }

            Packet.m_ClientID = -1;
            Packet.m_Address = pAddr;
            Packet.m_Flags = (int)NetworkConsts.NETSENDFLAG_CONNLESS;
            Packet.m_DataSize = p.Size();
            Packet.m_pData = p.Data();
            m_NetServer.Send(Packet);

            if (ShowMore && Take < 0)
                SendServerInfo(pAddr, Token, true, Offset + ClientsPerPacket);
        }

        void NewClientCallback(int ClientID)
        {
            m_aClients[ClientID].SetState(CClient.STATE_AUTH);
            m_aClients[ClientID].m_aName = "";
            m_aClients[ClientID].m_aClan = "";
            m_aClients[ClientID].m_Country = -1;
            m_aClients[ClientID].m_Authed = IConsole.ACCESS_LEVEL_NO;
            m_aClients[ClientID].m_AuthTries = 0;
            m_aClients[ClientID].m_pRconCmdToSend = null;
            m_aClients[ClientID].m_NonceCount = 0;
            m_aClients[ClientID].m_LastNonceCount = 0;
            m_aClients[ClientID].m_Traffic = 0;
            m_aClients[ClientID].m_TrafficSince = 0;

            m_aClients[ClientID].m_Addr.port = 0;
            m_aClients[ClientID].m_Addr.type = 0;
            m_aClients[ClientID].m_Addr.ip = null;
            m_aClients[ClientID].Reset();
        }

        void DelClientCallback(int ClientID, string pReason)
        {
            string aAddrStr = m_NetServer.ClientAddr(ClientID).IpStr;
            CSystem.dbg_msg_clr("clients", "client dropped. cid={0} addr={1} reason='{2}'", 
                ConsoleColor.DarkGreen, ClientID, aAddrStr, pReason);

            // notify the mod about the drop
            if (m_aClients[ClientID].State >= CClient.STATE_READY)
                m_pGameServer.OnClientDrop(ClientID, pReason);

            m_aClients[ClientID].SetState(CClient.STATE_EMPTY);
            m_aClients[ClientID].m_aName = null;
            m_aClients[ClientID].m_aClan = null;
            m_aClients[ClientID].m_Country = -1;
            m_aClients[ClientID].m_Authed = IConsole.ACCESS_LEVEL_NO;
            m_aClients[ClientID].m_AuthTries = 0;
            m_aClients[ClientID].m_pRconCmdToSend = null;
            m_aClients[ClientID].m_Traffic = 0;
            m_aClients[ClientID].m_TrafficSince = 0;
            m_aClients[ClientID].m_Snapshots.PurgeAll();
        }

        private void ConKick(CConsoleResult result, object data)
        {
        }

        private void ConBan(CConsoleResult result, object data)
        {
        }

        private void ConchainConsoleOutputLevelUpdate(CConsoleResult result, object data, FConsoleCallback callback, object userdata)
        {
        }

        private void ConchainModCommandUpdate(CConsoleResult result, object data, FConsoleCallback callback, object userdata)
        {
            
        }

        private void ConchainMaxclientsperipUpdate(CConsoleResult result, object data, FConsoleCallback callback, object userdata)
        {
            callback(result, userdata);
            if (result.NumArguments() > 0)
                m_NetServer.SetMaxClientsPerIP(result.GetInteger(0));
        }

        private void ConchainSpecialInfoupdate(CConsoleResult result, object data, FConsoleCallback callback, object userdata)
        {
            callback(result, userdata);
            if (result.NumArguments() > 0)
                UpdateServerInfo();
        }

        private void ConMapReload(CConsoleResult result, object data)
        {
        }

        private void ConShutdown(CConsoleResult result, object data)
        {
            
        }

        private void ConStatus(CConsoleResult result, object data)
        {
            
        }

        private void ConBans(CConsoleResult result, object data)
        {
            
        }

        private void ConUnban(CConsoleResult result, object data)
        {
            
        }

        private void RegisterCommands()
        {
            m_pConsole = Kernel.RequestInterface<IConsole>();
            m_pGameServer = Kernel.RequestInterface<IGameServer>();
            m_pMap = Kernel.RequestInterface<IEngineMap>();
            m_pStorage = Kernel.RequestInterface<IStorage>();

            Console.Register("kick", "i", CFGFLAG_SERVER, ConKick, this, "Kick player with specified id for any reason");
            Console.Register("ban", "s", CFGFLAG_SERVER | CFGFLAG_STORE, ConBan, this, "Ban player with ip/id for x minutes for any reason");
            Console.Register("unban", "s", CFGFLAG_SERVER | CFGFLAG_STORE, ConUnban, this, "Unban ip");
            Console.Register("bans", "", CFGFLAG_SERVER | CFGFLAG_STORE, ConBans, this, "Show banlist");
            Console.Register("status", "", CFGFLAG_SERVER, ConStatus, this, "List players");
            Console.Register("shutdown", "", CFGFLAG_SERVER, ConShutdown, this, "Shut down");

            //Console.Register("record", "?s", CFGFLAG_SERVER | CFGFLAG_STORE, ConRecord, this, "Record to a file");
            //Console.Register("stoprecord", "", CFGFLAG_SERVER, ConStopRecord, this, "Stop recording");

            Console.Register("reload", "", CFGFLAG_SERVER, ConMapReload, this, "Reload the map");

            Console.Chain("sv_name", ConchainSpecialInfoupdate, this);
            Console.Chain("password", ConchainSpecialInfoupdate, this);

            Console.Chain("sv_max_clients_per_ip", ConchainMaxclientsperipUpdate, this);
            Console.Chain("mod_command", ConchainModCommandUpdate, this);
            Console.Chain("console_output_level", ConchainConsoleOutputLevelUpdate, this);

            // register console commands in sub parts
            //m_ServerBan.InitServerBan(Console(), Storage(), this);
            m_pGameServer.OnConsoleInit();
        }

        private void InitRegister(CNetServer pNetServer, IMasterServer pMasterServer, IConsole pConsole)
        {
            m_Register.Init(pNetServer, pMasterServer, pConsole);    
        }

        private static void Main(string[] args)
        {
            System.Console.ForegroundColor = ConsoleColor.White;
            CServer pServer = CreateServer();
            IKernel pKernel = CKernel.Create();

            // create the components
            IEngine pEngine = CEngine.CreateEngine("Teeworlds");
            IEngineMap pEngineMap = CMap.CreateEngineMap();
            IGameServer pGameServer = CGameContext.CreateGameServer();
            IConsole pConsole = CConsole.CreateConsole();
            IMasterServer pEngineMasterServer = CMasterServer.CreateEngineMasterServer();
            IStorage pStorage = CStorage.CreateStorage("Teeworlds", IStorage.STORAGETYPE_SERVER, args); // ignore_convention
            IConfig pConfig = CConfig.CreateConfig();
            AppDomain.CurrentDomain.ProcessExit += pServer.CurrentDomainOnProcessExit;

            pServer.InitRegister(pServer.m_NetServer, pEngineMasterServer, pConsole);

            bool RegisterFail = false;
            RegisterFail = RegisterFail || !pKernel.RegisterInterface(pServer);
            RegisterFail = RegisterFail || !pKernel.RegisterInterface(pEngine);
            RegisterFail = RegisterFail || !pKernel.RegisterInterface(pEngineMap);
            RegisterFail = RegisterFail || !pKernel.RegisterInterface(pGameServer);
            RegisterFail = RegisterFail || !pKernel.RegisterInterface(pConsole);
            RegisterFail = RegisterFail || !pKernel.RegisterInterface(pStorage);
            RegisterFail = RegisterFail || !pKernel.RegisterInterface(pConfig);
            RegisterFail = RegisterFail || !pKernel.RegisterInterface(pEngineMasterServer);

            if (RegisterFail)
                return;

            pEngine.Init();
            pConfig.Init();
            pConsole.Init();
            pEngineMasterServer.Init();
            pEngineMasterServer.Load();

            // register all console commands
            pServer.RegisterCommands();

            // execute autoexec file
            pConsole.ExecuteFile("autoexec.cfg");
            pConsole.ParseArguments(args); // ignore_convention

            // restore empty config strings to their defaults
            pConfig.RestoreStrings();

            // run the server
            CSystem.dbg_msg("server", "starting...");
            pServer.Run();
        }

        private void CurrentDomainOnProcessExit(object sender, EventArgs eventArgs)
        {
            
        }

        private static CServer CreateServer()
        {
            return new CServer();
        }

        public CServer()
        {
            m_RunServer = true;
            m_Register = new CRegister();
            m_NetServer = new CNetServer();
            m_IDPool = new CSnapIDPool();

            m_SnapshotDelta = new CSnapshotDelta();
            m_SnapshotBuilder = new CSnapshotBuilder();

            m_TickSpeed = (int)Consts.SERVER_TICK_SPEED;

            Init();
        }

        private void Init()
        {
            m_aClients = new CClient[MAX_CLIENTS];
            for (int i = 0; i < MAX_CLIENTS; i++)
            {
                m_aClients[i] = new CClient();
                m_aClients[i].SetState(CClient.STATE_EMPTY);
                m_aClients[i].m_aName = "";
                m_aClients[i].m_aClan = "";
                m_aClients[i].m_Country = -1;
                m_aClients[i].m_Snapshots.Init();
                m_aClients[i].m_Traffic = 0;
                m_aClients[i].m_TrafficSince = 0;
            }

            m_CurrentGameTick = 0;
        }
    }
}

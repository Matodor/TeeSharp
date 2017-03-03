using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teecsharp
{
    public class CRegister
    {
        private const int
            REGISTERSTATE_START = 0,
            REGISTERSTATE_UPDATE_ADDRS = 1,
            REGISTERSTATE_QUERY_COUNT = 2,
            REGISTERSTATE_HEARTBEAT = 3,
            REGISTERSTATE_REGISTERED = 4,
            REGISTERSTATE_ERROR = 5;

        private class CMasterserverInfo
        {
            public NETADDR m_Addr = new NETADDR();
            public int m_Count;
            public int m_Valid;
            public long m_LastSend;
        }

        private CNetServer m_pNetServer;
        private IMasterServer m_pMasterServer;
        private IConsole m_pConsole;
        private readonly CConfiguration g_Config;
        private int m_RegisterState;
        private long m_RegisterStateStart;
        private int m_RegisterFirst;
        private int m_RegisterCount;
        
        private readonly CMasterserverInfo[] m_aMasterserverInfo = new CMasterserverInfo[IMasterServer.MAX_MASTERSERVERS];
        private int m_RegisterRegisteredServer;

        public CRegister()
        {
            g_Config = CConfiguration.Instance;
            m_pNetServer = null;
            m_pMasterServer = null;
            m_pConsole = null;

            m_RegisterState = REGISTERSTATE_START;
            m_RegisterStateStart = 0;
            m_RegisterFirst = 1;
            m_RegisterCount = 0;

            for (int i = 0; i < m_aMasterserverInfo.Length; i++)
                m_aMasterserverInfo[i] = new CMasterserverInfo();
            m_RegisterRegisteredServer = -1;
        }

        void RegisterNewState(int State)
        {
            m_RegisterState = State;
            m_RegisterStateStart = CSystem.time_get();
        }

        void RegisterSendFwcheckresponse(NETADDR pAddr)
        {
            CNetChunk Packet = new CNetChunk();
            Packet.m_ClientID = -1;
            Packet.m_Address = pAddr;
            Packet.m_Flags = (int)NetworkConsts.NETSENDFLAG_CONNLESS;
            Packet.m_DataSize = CMasterServer.SERVERBROWSE_FWRESPONSE.Length;
            Packet.m_pData = CMasterServer.SERVERBROWSE_FWRESPONSE;
            m_pNetServer.Send(Packet);
        }

        public void RegisterSendHeartbeat(NETADDR Addr)
        {
            int Port = g_Config.GetInt("SvPort");
            byte[] aData = new byte[CMasterServer.SERVERBROWSE_HEARTBEAT.Length + 2];
            Array.Copy(CMasterServer.SERVERBROWSE_HEARTBEAT, 0, aData, 0, CMasterServer.SERVERBROWSE_HEARTBEAT.Length);

            CNetChunk Packet = new CNetChunk();
            Packet.m_ClientID = -1;
            Packet.m_Address = Addr;
            Packet.m_Flags = (int)NetworkConsts.NETSENDFLAG_CONNLESS;
            Packet.m_DataSize = aData.Length;
            Packet.m_pData = aData;

            // supply the set port that the master can use if it has problems
            if (g_Config.GetInt("SvExternalPort") != 0)
                Port = g_Config.GetInt("SvExternalPort");
            Packet.m_pData[CMasterServer.SERVERBROWSE_HEARTBEAT.Length] = (byte) (Port >> 8);
            Packet.m_pData[CMasterServer.SERVERBROWSE_HEARTBEAT.Length + 1] = (byte) (Port & 0xff);
            m_pNetServer.Send(Packet);
        }

        public void RegisterSendCountRequest(NETADDR Addr)
        {
            CNetChunk Packet = new CNetChunk();
            Packet.m_ClientID = -1;
            Packet.m_Address = Addr;
            Packet.m_Flags = (int)NetworkConsts.NETSENDFLAG_CONNLESS;
            Packet.m_DataSize = CMasterServer.SERVERBROWSE_GETCOUNT.Length;
            Packet.m_pData = CMasterServer.SERVERBROWSE_GETCOUNT;
            m_pNetServer.Send(Packet);
        }

        public void RegisterGotCount(CNetChunk pChunk)
        {
            byte[] pData = pChunk.m_pData;
            int Count = (pData[CMasterServer.SERVERBROWSE_COUNT.Length] << 8) | pData[CMasterServer.SERVERBROWSE_COUNT.Length + 1];

            for (int i = 0; i < IMasterServer.MAX_MASTERSERVERS; i++)
            {
                if (CSystem.net_addr_comp(m_aMasterserverInfo[i].m_Addr, pChunk.m_Address))
                {
                    CSystem.dbg_msg("mastersrv", "{0} got count {1}", m_pMasterServer.GetName(i), Count);
                    m_aMasterserverInfo[i].m_Count = Count;
                    break;
                }
            }
        }

        public void Init(CNetServer pNetServer, IMasterServer pMasterServer, IConsole pConsole)
        {
            m_pNetServer = pNetServer;
            m_pMasterServer = pMasterServer;
            m_pConsole = pConsole;
        }

        public void RegisterUpdate(int Nettype)
        {
            var Now = CSystem.time_get();
            var Freq = CSystem.time_freq();

            if (g_Config.GetInt("SvRegister") == 0)
                return;

            m_pMasterServer.Update();

            if (m_RegisterState == REGISTERSTATE_START)
            {
                m_RegisterCount = 0;
                m_RegisterFirst = 1;
                RegisterNewState(REGISTERSTATE_UPDATE_ADDRS);
                m_pMasterServer.RefreshAddresses(Nettype);
                m_pConsole.Print(IConsole.OUTPUT_LEVEL_STANDARD, "register", "refreshing ip addresses");
            }
            else if (m_RegisterState == REGISTERSTATE_UPDATE_ADDRS)
            {
                m_RegisterRegisteredServer = -1;

                if (m_pMasterServer.IsRefreshing() == 0)
                {
                    int i;
                    for (i = 0; i < IMasterServer.MAX_MASTERSERVERS; i++)
                    {
                        if (!m_pMasterServer.IsValid(i))
                        {
                            m_aMasterserverInfo[i].m_Valid = 0;
                            m_aMasterserverInfo[i].m_Count = 0;
                            continue;
                        }

                        NETADDR Addr = m_pMasterServer.GetAddr(i);
                        m_aMasterserverInfo[i].m_Addr = Addr;
                        m_aMasterserverInfo[i].m_Valid = 1;
                        m_aMasterserverInfo[i].m_Count = -1;
                        m_aMasterserverInfo[i].m_LastSend = 0;
                    }

                    m_pConsole.Print(IConsole.OUTPUT_LEVEL_STANDARD, "register", "fetching server counts");
                    RegisterNewState(REGISTERSTATE_QUERY_COUNT);
                }
            }
            else if (m_RegisterState == REGISTERSTATE_QUERY_COUNT)
            {
                int Left = 0;
                for (int i = 0; i < IMasterServer.MAX_MASTERSERVERS; i++)
                {
                    if (m_aMasterserverInfo[i].m_Valid == 0)
                        continue;

                    if (m_aMasterserverInfo[i].m_Count == -1)
                    {
                        Left++;
                        if (m_aMasterserverInfo[i].m_LastSend + Freq < Now)
                        {
                            m_aMasterserverInfo[i].m_LastSend = Now;
                            RegisterSendCountRequest(m_aMasterserverInfo[i].m_Addr);
                        }
                    }
                }

                // check if we are done or timed out
                if (Left == 0 || Now > m_RegisterStateStart + Freq * 3)
                {
                    // choose server
                    int Best = -1;
                    int i;
                    for (i = 0; i < IMasterServer.MAX_MASTERSERVERS; i++)
                    {
                        if (m_aMasterserverInfo[i].m_Valid == 0 || m_aMasterserverInfo[i].m_Count == -1)
                            continue;

                        if (Best == -1 || m_aMasterserverInfo[i].m_Count < m_aMasterserverInfo[Best].m_Count)
                            Best = i;
                    }

                    // server chosen
                    m_RegisterRegisteredServer = Best;
                    if (m_RegisterRegisteredServer == -1)
                    {
                        m_pConsole.Print(IConsole.OUTPUT_LEVEL_STANDARD, "register", "WARNING: No master servers. Retrying in 60 seconds");
                        RegisterNewState(REGISTERSTATE_ERROR);
                    }
                    else
                    {
                        string aBuf = string.Format("chose '{0}' as master, sending heartbeats", m_pMasterServer.GetName(m_RegisterRegisteredServer));
                        m_pConsole.Print(IConsole.OUTPUT_LEVEL_STANDARD, "register", aBuf);
                        m_aMasterserverInfo[m_RegisterRegisteredServer].m_LastSend = 0;
                        RegisterNewState(REGISTERSTATE_HEARTBEAT);
                    }
                }
            }
            else if (m_RegisterState == REGISTERSTATE_HEARTBEAT)
            {
                if (m_RegisterRegisteredServer == -1)
                {
                    RegisterNewState(REGISTERSTATE_START);
                    return;
                }

                // check if we should send heartbeat
                if (Now > m_aMasterserverInfo[m_RegisterRegisteredServer].m_LastSend + Freq * 15)
                {
                    m_aMasterserverInfo[m_RegisterRegisteredServer].m_LastSend = Now;
                    RegisterSendHeartbeat(m_aMasterserverInfo[m_RegisterRegisteredServer].m_Addr);
                }

                if (Now > m_RegisterStateStart + Freq * 60)
                {
                    m_pConsole.Print(IConsole.OUTPUT_LEVEL_STANDARD, "register", "WARNING: Master server is not responding, switching master");
                    RegisterNewState(REGISTERSTATE_START);
                }
            }
            else if (m_RegisterState == REGISTERSTATE_REGISTERED)
            {
                if (m_RegisterFirst != 0)
                    m_pConsole.Print(IConsole.OUTPUT_LEVEL_STANDARD, "register", "server registered");

                m_RegisterFirst = 0;

                // check if we should send new heartbeat again
                if (Now > m_RegisterStateStart + Freq)
                {
                    if (m_RegisterCount == 120) // redo the whole process after 60 minutes to balance out the master servers
                        RegisterNewState(REGISTERSTATE_START);
                    else
                    {
                        m_RegisterCount++;
                        RegisterNewState(REGISTERSTATE_HEARTBEAT);
                    }
                }
            }
            else if (m_RegisterState == REGISTERSTATE_ERROR)
            {
                // check for restart
                if (Now > m_RegisterStateStart + Freq * 60)
                    RegisterNewState(REGISTERSTATE_START);
            }
        }

        public bool RegisterProcessPacket(CNetChunk pPacket)
        {
            // check for masterserver address
            bool Valid = false;

            for (int i = 0; i < IMasterServer.MAX_MASTERSERVERS; i++)
            {
                if (CSystem.net_addr_comp(pPacket.m_Address, m_aMasterserverInfo[i].m_Addr, false))
                {
                    Valid = true;
                    break;
                }
            }
            if (!Valid)
                return false;

            if (pPacket.m_DataSize == CMasterServer.SERVERBROWSE_FWCHECK.Length &&
                CSystem.mem_comp(pPacket.m_pData, CMasterServer.SERVERBROWSE_FWCHECK))
            {
                RegisterSendFwcheckresponse(pPacket.m_Address);
                return true;
            }

            if (pPacket.m_DataSize == CMasterServer.SERVERBROWSE_FWOK.Length &&
                CSystem.mem_comp(pPacket.m_pData, CMasterServer.SERVERBROWSE_FWOK))
            {
                if (m_RegisterFirst != 0)
                    m_pConsole.Print(IConsole.OUTPUT_LEVEL_STANDARD, "register", "no firewall/nat problems detected");
                RegisterNewState(REGISTERSTATE_REGISTERED);
                return true;
            }

            if (pPacket.m_DataSize == CMasterServer.SERVERBROWSE_FWERROR.Length &&
                CSystem.mem_comp(pPacket.m_pData, CMasterServer.SERVERBROWSE_FWERROR))
            {
                m_pConsole.Print(IConsole.OUTPUT_LEVEL_STANDARD, "register", "ERROR: the master server reports that clients can not connect to this server.");
                string aBuf = string.Format("ERROR: configure your firewall/nat to let through udp on port {0}.", g_Config.GetInt("SvPort"));
                m_pConsole.Print(IConsole.OUTPUT_LEVEL_STANDARD, "register", aBuf);
                RegisterNewState(REGISTERSTATE_ERROR);
                return true;
            }

            if (pPacket.m_DataSize == CMasterServer.SERVERBROWSE_COUNT.Length + 2 &&
                CSystem.mem_comp(pPacket.m_pData, CMasterServer.SERVERBROWSE_COUNT))
            {
                RegisterGotCount(pPacket);
                return true;
            }

            return false;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teecsharp
{
    public class CNetServer
    {
        public int NetType() { return m_Socket.type; }

        private class CSlot
        {
            public readonly CNetConnection m_Connection;

            public CSlot()
            {
                m_Connection = new CNetConnection();
            }
        }

        private readonly CNetRecvUnpacker m_RecvUnpacker;
        private NETSOCKET m_Socket;
        private CNetBan m_pNetBan;
        private readonly CSlot[] m_aSlots;
        private int m_MaxClients;
        private int m_MaxClientsPerIP;

        private Action<int> m_pfnNewClient;
        private Action<int, string> m_pfnDelClient;

        public CNetServer()
        {
            m_aSlots = new CSlot[(int)NetworkConsts.NET_MAX_CLIENTS];
            m_RecvUnpacker = new CNetRecvUnpacker();
        }

        public bool Open(NETADDR BindAddr, CNetBan pNetBan, int MaxClients, int MaxClientsPerIP,
            int Flags)
        {
            // open socket
            m_Socket = CSystem.net_udp_create(BindAddr);

            if (m_Socket.type == 0)
                return false;

            m_pNetBan = pNetBan;

            // clamp clients
            m_MaxClients = MaxClients;
            if (m_MaxClients > (int)NetworkConsts.NET_MAX_CLIENTS)
                m_MaxClients = (int)NetworkConsts.NET_MAX_CLIENTS;
            if (m_MaxClients < 1)
                m_MaxClients = 1;

            m_MaxClientsPerIP = MaxClientsPerIP;

            for (int i = 0; i < (int)NetworkConsts.NET_MAX_CLIENTS; i++)
            {
                m_aSlots[i] = new CSlot();
                m_aSlots[i].m_Connection.Init(m_Socket, true);
            }
            return true;
        }

        public void SetMaxClientsPerIP(int Max)
        {
            // clamp
            if (Max < 1)
                Max = 1;
            else if (Max > (int)NetworkConsts.NET_MAX_CLIENTS)
                Max = (int)NetworkConsts.NET_MAX_CLIENTS;

            m_MaxClientsPerIP = Max;
        }

        public void SetCallbacks(Action<int> pfnNewClient, Action<int, string> pfnDelClient)
        {
            m_pfnNewClient = pfnNewClient;
            m_pfnDelClient = pfnDelClient;
        }

        public CNetBan NetBan()
        {
            return m_pNetBan;
        }

        public int MaxClients()
        {
            return m_MaxClients;
        }

        public NETADDR ClientAddr(int ClientID)
        {
            return m_aSlots[ClientID].m_Connection.PeerAddress();
        }

        public void Drop(int ClientID, string pReason)
        {
            // TODO: insert lots of checks here
            NETADDR Addr = ClientAddr(ClientID);
            CSystem.dbg_msg("net_server", "client dropped. cid={0} ip={1} reason=\"{2}\"", ClientID, Addr.IpStr, pReason);
            
            m_pfnDelClient?.Invoke(ClientID, pReason);
            m_aSlots[ClientID].m_Connection.Disconnect(pReason);
        }

        public void Update()
        {
            long Now = CSystem.time_get();
            for (int i = 0; i < MaxClients(); i++)
            {
                m_aSlots[i].m_Connection.Update();
                if (m_aSlots[i].m_Connection.State() == (int)NetworkConsts.NET_CONNSTATE_ERROR)
                {
                    if (Now - m_aSlots[i].m_Connection.ConnectTime() < CSystem.time_freq() && NetBan() != null)
                        NetBan().BanAddr(ClientAddr(i), 60, "Too many connections");
                    else
                        Drop(i, m_aSlots[i].m_Connection.ErrorString());
                }
            }
        }

        public bool Recv(CNetChunk pChunk)
        {
            while (true)
            {
                NETADDR Addr = new NETADDR();

                // check for a chunk
                if (m_RecvUnpacker.FetchChunk(pChunk))
                    return true;

                // TODO: empty the recvinfo
                int Bytes = CSystem.net_udp_recv(m_Socket, ref Addr, m_RecvUnpacker.m_aBuffer,
                    (int)NetworkConsts.NET_MAX_PACKETSIZE);

                // no more packets for now
                if (Bytes <= 0)
                    break;

                // check if we just should drop the packet
                string aBuf = "";
                if (NetBan() != null && NetBan().IsBanned(Addr, aBuf))
                {
                    // banned, reply with a message
                    CNetBase.SendControlMsg(m_Socket, Addr, 0, (int)NetworkConsts.NET_CTRLMSG_CLOSE, aBuf);
                    continue;
                }

                if (CNetBase.UnpackPacket(m_RecvUnpacker.m_aBuffer, Bytes, m_RecvUnpacker.m_Data))
                {
                    if ((m_RecvUnpacker.m_Data.m_Flags & (int)NetworkConsts.NET_PACKETFLAG_CONNLESS) != 0)
                    {
                        pChunk.m_Flags = (int)NetworkConsts.NETSENDFLAG_CONNLESS;
                        pChunk.m_ClientID = -1;
                        pChunk.m_Address = Addr;
                        pChunk.m_DataSize = m_RecvUnpacker.m_Data.m_DataSize;
                        pChunk.m_pData = m_RecvUnpacker.m_Data.m_aChunkData;
                        return true;
                    }

                    // TODO: check size here
                    if ((m_RecvUnpacker.m_Data.m_Flags & (int)NetworkConsts.NET_PACKETFLAG_CONTROL) != 0 &&
                        m_RecvUnpacker.m_Data.m_aChunkData[0] == (int)NetworkConsts.NET_CTRLMSG_CONNECT)
                    {
                        var Found = false;

                        // check if we already got this client
                        for (int i = 0; i < MaxClients(); i++)
                        {
                            if (m_aSlots[i].m_Connection.State() != (int)NetworkConsts.NET_CONNSTATE_OFFLINE &&
                                m_aSlots[i].m_Connection.State() != (int)NetworkConsts.NET_CONNSTATE_ERROR &&
                                CSystem.net_addr_comp(m_aSlots[i].m_Connection.PeerAddress(), Addr))
                            {
                                Found = true; // silent ignore.. we got this client already
                                //if(m_aSlots[i].m_Connection.State() == NET_CONNSTATE_ERROR)
                                //{
                                //	m_aSlots[i].m_Connection.Feed(&m_RecvUnpacker.m_Data, &Addr);
                                //	if(m_pfnNewClient)
                                //		m_pfnNewClient(i, m_UserPtr);
                                //}
                                break;
                            }
                        }

                        // client that wants to connect
                        if (!Found)
                        {
                            // only allow a specific number of players with the same ip
                            int FoundAddr = 1;

                            for (int i = 0; i < MaxClients(); ++i)
                            {
                                if (m_aSlots[i].m_Connection.State() == (int)NetworkConsts.NET_CONNSTATE_OFFLINE)
                                    continue;

                                //OtherAddr = m_aSlots[i].m_Connection.PeerAddress();
                                //OtherAddr.port = 0;
                                if (CSystem.net_addr_comp(m_aSlots[i].m_Connection.PeerAddress(), Addr, false))
                                {
                                    if (FoundAddr++ >= m_MaxClientsPerIP)
                                    {
                                        aBuf = string.Format("Only {0} players with the same IP are allowed",
                                            m_MaxClientsPerIP);
                                        CNetBase.SendControlMsg(m_Socket, Addr, 0, (int)NetworkConsts.NET_CTRLMSG_CLOSE, aBuf);
                                        return false;
                                    }
                                }
                            }

                            for (int i = 0; i < MaxClients(); i++)
                            {
                                if (m_aSlots[i].m_Connection.State() == (int)NetworkConsts.NET_CONNSTATE_OFFLINE)
                                {
                                    Found = true;
                                    m_aSlots[i].m_Connection.Feed(m_RecvUnpacker.m_Data, Addr);
                                    m_pfnNewClient?.Invoke(i);
                                    break;
                                }
                            }

                            if (!Found)
                            {
                                string FullMsg = "This server is full";
                                CNetBase.SendControlMsg(m_Socket, Addr, 0, (int)NetworkConsts.NET_CTRLMSG_CLOSE, FullMsg);
                            }
                        }
                    }
                    else
                    {
                        // normal packet, find matching slot
                        for (int i = 0; i < MaxClients(); i++)
                        {
                            if (CSystem.net_addr_comp(m_aSlots[i].m_Connection.PeerAddress(), Addr))
                            {
                                if (m_aSlots[i].m_Connection.State() == (int)NetworkConsts.NET_CONNSTATE_OFFLINE ||
                                    m_aSlots[i].m_Connection.State() == (int)NetworkConsts.NET_CONNSTATE_ERROR)
                                    continue;
                                if (m_aSlots[i].m_Connection.Feed(m_RecvUnpacker.m_Data, Addr))
                                {
                                    if (m_RecvUnpacker.m_Data.m_DataSize != 0)
                                        m_RecvUnpacker.Start(Addr, m_aSlots[i].m_Connection, i);
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
        
        public bool Send(CNetChunk pChunk)
        {
            if (pChunk.m_DataSize >= (int)NetworkConsts.NET_MAX_PAYLOAD)
            {
                CSystem.dbg_msg("netserver", "packet payload too big. {0}. dropping packet", pChunk.m_DataSize);
                return false;
            }

            if ((pChunk.m_Flags & (int)NetworkConsts.NETSENDFLAG_CONNLESS) != 0)
            {
                // send connectionless packet
                CNetBase.SendPacketConnless(m_Socket, pChunk.m_Address, pChunk.m_pData, pChunk.m_DataSize);
            }
            else
            {
                int Flags = 0;
                //dbg_assert(pChunk->m_ClientID >= 0, "errornous client id");
                //dbg_assert(pChunk->m_ClientID < MaxClients(), "errornous client id");

                if ((pChunk.m_Flags & (int)NetworkConsts.NETSENDFLAG_VITAL) != 0)
                    Flags = (int)NetworkConsts.NET_CHUNKFLAG_VITAL;

                m_aSlots[pChunk.m_ClientID].m_Connection.QueueChunk(Flags, pChunk.m_DataSize,
                    pChunk.m_pData);
                
                if ((pChunk.m_Flags & (int)NetworkConsts.NETSENDFLAG_FLUSH) != 0)
                    m_aSlots[pChunk.m_ClientID].m_Connection.Flush();
            }
            return true;
        }
    }
}

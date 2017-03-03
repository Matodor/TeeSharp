using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Teecsharp
{
    public class CNetConnection
    {
        // TODO: is this needed because this needs to be aware of
        // the ack sequencing number and is also responible for updating
        // that. this should be fixed.
        public ushort m_Ack;

        private ushort m_Sequence;
        private uint m_State;

        private int m_RemoteClosed;
        private bool m_BlockCloseMsg;

        private static CConfiguration g_Config;
        private readonly Queue<CNetChunkResend> m_Buffer;
        private CNetPacketConstruct m_Construct;
        private NETADDR m_PeerAddr;
        private NETSOCKET m_Socket;

        private long m_LastUpdateTime;
        private long m_LastRecvTime;
        private long m_LastSendTime;
        private string m_ErrorString;

        public CNetConnection()
        {
            m_Buffer = new Queue<CNetChunkResend>();
            g_Config = CConfiguration.Instance;
        }

        public void Init(NETSOCKET Socket, bool BlockCloseMsg)
        {
            Reset();

            m_Socket = Socket;
            m_BlockCloseMsg = BlockCloseMsg;
            m_ErrorString = "";
        }

        public void Reset()
        {
            m_Sequence = 0;
            m_Ack = 0;
            m_RemoteClosed = 0;
            m_PeerAddr = new NETADDR();
            m_State = (int)NetworkConsts.NET_CONNSTATE_OFFLINE;
            m_LastSendTime = 0;
            m_LastRecvTime = 0;
            m_LastUpdateTime = 0;
            m_Buffer.Clear();
            m_Construct = new CNetPacketConstruct();
        }

        public NETADDR PeerAddress()
        {
            return m_PeerAddr;
        }

        public uint State()
        {
            return m_State;
        }

        private void SetError(string pString)
        {
            m_ErrorString = pString;
        }

        private void ResendChunk(CNetChunkResend pResend)
        {
            QueueChunkEx(pResend.m_Flags | (int)NetworkConsts.NET_CHUNKFLAG_RESEND, pResend.m_DataSize,
                pResend.m_pData, pResend.m_Sequence);
            pResend.m_LastSendTime = CSystem.time_get();
        }

        private void Resend()
        {
            if (m_Buffer.Count > 0)
            {
                CSystem.dbg_msg("server", "resend chunks");
                foreach (CNetChunkResend chunkResend in m_Buffer)
                {
                    ResendChunk(chunkResend);
                }
            }
        }

        public bool Feed(CNetPacketConstruct pPacket, NETADDR pAddr)
        {
            long Now = CSystem.time_get();

            // check if resend is requested
            if ((pPacket.m_Flags & (int)NetworkConsts.NET_PACKETFLAG_RESEND) != 0)
                Resend();

            //
            if ((pPacket.m_Flags & (int)NetworkConsts.NET_PACKETFLAG_CONTROL) != 0)
            {
                var CtrlMsg = pPacket.m_aChunkData[0];
                if (CtrlMsg == (int)NetworkConsts.NET_CTRLMSG_CLOSE)
                {
                    if (CSystem.net_addr_comp(m_PeerAddr, pAddr))
                    {
                        m_State = (int)NetworkConsts.NET_CONNSTATE_ERROR;
                        m_RemoteClosed = 1;

                        string Str = "";
                        if (pPacket.m_DataSize != 1)
                        {
                            // make sure to sanitize the error string form the other party
                            //if (pPacket.m_DataSize < 128)
                            //    str_copy(Str, (char*)&pPacket.m_aChunkData[1], pPacket.m_DataSize);
                            //else
                            //    str_copy(Str, (char*)&pPacket.m_aChunkData[1], sizeof(Str));
                            //str_sanitize_strong(Str);
                        }

                        if (!m_BlockCloseMsg)
                        {
                            // set the error string
                            SetError(Str);
                        }

                        if (g_Config.GetInt("Debug") != 0)
                            CSystem.dbg_msg_clr("conn", "closed reason='{0}'", ConsoleColor.Green, Str);
                    }
                    return false;
                }
                
                if (State() == (int)NetworkConsts.NET_CONNSTATE_OFFLINE)
                {
                    if (CtrlMsg == (int)NetworkConsts.NET_CTRLMSG_CONNECT)
                    {
                        // send response and init connection
                        Reset();
                        m_State = (int)NetworkConsts.NET_CONNSTATE_PENDING;
                        m_PeerAddr = pAddr;
                        m_ErrorString = "";
                        m_LastSendTime = Now;
                        m_LastRecvTime = Now;
                        m_LastUpdateTime = Now;
                        SendControl((int)NetworkConsts.NET_CTRLMSG_CONNECTACCEPT);
                        if (g_Config.GetInt("Debug") != 0)
                            CSystem.dbg_msg_clr("connection", "got connection, sending connect+accept", ConsoleColor.Green);
                    }
                }
                else if (State() == (int)NetworkConsts.NET_CONNSTATE_CONNECT)
                {
                    // connection made
                    if (CtrlMsg == (int)NetworkConsts.NET_CTRLMSG_CONNECTACCEPT)
                    {
                        m_LastRecvTime = Now;
                        SendControl((int)NetworkConsts.NET_CTRLMSG_ACCEPT);
                        m_State = (int)NetworkConsts.NET_CONNSTATE_ONLINE;

                        if (g_Config.GetInt("Debug") != 0)
                            CSystem.dbg_msg_clr("connection", "got connect+accept, sending accept. connection online", ConsoleColor.Green);
                    }
                }
            }
            else
            {
                if (State() == (int)NetworkConsts.NET_CONNSTATE_PENDING)
                {
                    m_LastRecvTime = Now;
                    m_State = (int)NetworkConsts.NET_CONNSTATE_ONLINE;
                    //if (g_Config.m_Debug)
                    CSystem.dbg_msg_clr("connection", "connecting online", ConsoleColor.Green);
                }
            }

            if (State() == (int)NetworkConsts.NET_CONNSTATE_ONLINE)
            {
                m_LastRecvTime = Now;
                AckChunks(pPacket.m_Ack);
            }

            return true;
        }

        private void AckChunks(int Ack)
        {
            while (true)
            {
                if (m_Buffer.Count == 0)
                    break;
                CNetChunkResend pResend = m_Buffer.Peek();
                if (pResend == null)
                    break;

                if (CNetBase.IsSeqInBackroom(pResend.m_Sequence, Ack))
                    m_Buffer.Dequeue();
                else
                    return;
            }
        }

        public int Flush()
        {
            int NumChunks = m_Construct.m_NumChunks;
            if (NumChunks == 0 && m_Construct.m_Flags == 0)
                return 0;
            
            // send of the packets
            m_Construct.m_Ack = m_Ack;
            CNetBase.SendPacket(m_Socket, m_PeerAddr, m_Construct);

            // update send times
            m_LastSendTime = CSystem.time_get();

            // clear construct so we can start building a new package
            m_Construct = null;
            m_Construct = new CNetPacketConstruct();

            return NumChunks;
        }

        public void QueueChunk(int Flags, int DataSize, byte[] pData)
        {
            if ((Flags & (int)NetworkConsts.NET_CHUNKFLAG_VITAL) != 0)
                m_Sequence = (ushort)((m_Sequence + 1) % (int)NetworkConsts.NET_MAX_SEQUENCE);
            QueueChunkEx(Flags, DataSize, pData, m_Sequence);
        }

        private void QueueChunkEx(int Flags, int DataSize, byte[] pData, int Sequence)
        {
            // check if we have space for it, if not, flush the connection
            if (m_Construct.m_DataSize + DataSize + (int)NetworkConsts.NET_MAX_CHUNKHEADERSIZE > m_Construct.m_aChunkData.Length)
                Flush();

            // pack all the data
            CNetChunkHeader Header = new CNetChunkHeader
            {
                m_Flags = Flags,
                m_Size = DataSize,
                m_Sequence = Sequence
            };

            int pChunkDataIndex = m_Construct.m_DataSize;
            pChunkDataIndex = Header.Pack(m_Construct.m_aChunkData, pChunkDataIndex);

            Array.Copy(pData, 0, m_Construct.m_aChunkData, pChunkDataIndex, DataSize);
            pChunkDataIndex += DataSize;

            //
            m_Construct.m_NumChunks++;
            m_Construct.m_DataSize = pChunkDataIndex;

            // set packet flags aswell
            if ((Flags & (int)NetworkConsts.NET_CHUNKFLAG_VITAL) != 0 && (Flags & (int)NetworkConsts.NET_CHUNKFLAG_RESEND) == 0)
            {
                // save packet if we need to resend

                CNetChunkResend pResend = new CNetChunkResend
                {
                    m_Sequence = Sequence,
                    m_Flags = Flags,
                    m_DataSize = DataSize,
                    m_pData = new byte[DataSize],
                    m_FirstSendTime = CSystem.time_get()
                };
                pResend.m_LastSendTime = pResend.m_FirstSendTime;
                Array.Copy(pData, 0, pResend.m_pData, 0, DataSize);
                m_Buffer.Enqueue(pResend);
            }
        }

        public void Update()
        {
            long Now = CSystem.time_get();

            if (State() == (int)NetworkConsts.NET_CONNSTATE_OFFLINE || State() == (int)NetworkConsts.NET_CONNSTATE_ERROR)
                return;

            // check for timeout
            if (State() != (int)NetworkConsts.NET_CONNSTATE_OFFLINE &&
                State() != (int)NetworkConsts.NET_CONNSTATE_CONNECT &&
                (Now - m_LastRecvTime) > CSystem.time_freq() * g_Config.GetInt("ConnTimeout")) // TODO
            {
                m_State = (int)NetworkConsts.NET_CONNSTATE_ERROR;
                SetError("Timeout");
            }

            // fix resends
            if (m_Buffer.Count > 0)
            {
                CNetChunkResend pResend = m_Buffer.Peek();

                // check if we have some really old stuff laying around and abort if not acked
                if (Now - pResend.m_FirstSendTime > CSystem.time_freq() * g_Config.GetInt("ConnTimeout"))
                {
                    m_State = (int)NetworkConsts.NET_CONNSTATE_ERROR;
                    SetError($"Too weak connection (not acked for {100} seconds)");
                }
                else
                {
                    // resend packet if we havn't got it acked in 1 second
                    if (Now - pResend.m_LastSendTime > CSystem.time_freq())
                        ResendChunk(pResend);
                }
            }

            // send keep alives if nothing has happend for 250ms
            if (State() == (int)NetworkConsts.NET_CONNSTATE_ONLINE)
            {
                if (Now - m_LastSendTime > CSystem.time_freq() / 2) // flush connection after 500ms if needed
                {
                    int NumFlushedChunks = Flush();
                    if (NumFlushedChunks != 0 && g_Config.GetInt("Debug") != 0)
                        CSystem.dbg_msg("connection", "flushed connection due to timeout. {0} chunks.", NumFlushedChunks);
                }

                if (Now - m_LastSendTime > CSystem.time_freq())
                    SendControl((int)NetworkConsts.NET_CTRLMSG_KEEPALIVE);
            }
            else if (State() == (int)NetworkConsts.NET_CONNSTATE_CONNECT)
            {
                if (Now - m_LastSendTime > CSystem.time_freq() / 2) // send a new connect every 500ms
                    SendControl((int)NetworkConsts.NET_CTRLMSG_CONNECT);
            }
            else if (State() == (int)NetworkConsts.NET_CONNSTATE_PENDING)
            {
                if (Now - m_LastSendTime > CSystem.time_freq() / 2) // send a new connect/accept every 500ms
                    SendControl((int)NetworkConsts.NET_CTRLMSG_CONNECTACCEPT);
            }
        }

        public void Disconnect(string pReason)
        {
            if (State() == (int)NetworkConsts.NET_CONNSTATE_OFFLINE)
                return;

            if (m_RemoteClosed == 0)
            {
                SendControl((int)NetworkConsts.NET_CTRLMSG_CLOSE, pReason);
                m_ErrorString = "";

                if (!string.IsNullOrEmpty(pReason))
                    m_ErrorString = pReason;
            }

            Reset();
        }

        public void SendControl(byte ControlMsg, string message = "")
        {
            // send the control message
            m_LastSendTime = CSystem.time_get();
            CNetBase.SendControlMsg(m_Socket, m_PeerAddr, m_Ack, ControlMsg, message);
        }

        public string ErrorString()
        {
            return m_ErrorString;
        }

        public long ConnectTime()
        {
            return m_LastUpdateTime;
        }

        public void SignalResend()
        {
            m_Construct.m_Flags |= (int)NetworkConsts.NET_PACKETFLAG_RESEND;
        }
    }
}

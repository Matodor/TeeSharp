using System;
using System.Text;

namespace Teecsharp
{
    enum NetworkConsts
    {
        NETADDR_MAXSTRSIZE = 1 + (8 * 4 + 7) + 1 + 1 + 5 + 1, // [XXXX:XXXX:XXXX:XXXX:XXXX:XXXX:XXXX:XXXX]:XXXXX

        NETTYPE_INVALID = 0,
        NETTYPE_IPV4 = 1,
        NETTYPE_IPV6 = 2,
        NETTYPE_LINK_BROADCAST = 4,
        NETTYPE_ALL = NETTYPE_IPV4 | NETTYPE_IPV6,

        NETFLAG_ALLOWSTATELESS = 1,
        NETSENDFLAG_VITAL = 1,
        NETSENDFLAG_CONNLESS = 2,
        NETSENDFLAG_FLUSH = 4,

        NETSTATE_OFFLINE = 0,
        NETSTATE_CONNECTING,
        NETSTATE_ONLINE,

        NETBANTYPE_SOFT = 1,
        NETBANTYPE_DROP = 2,

        NET_VERSION = 2,

        NET_MAX_PACKETSIZE = 1400,
        NET_MAX_PAYLOAD = NET_MAX_PACKETSIZE - 6,
        NET_MAX_CHUNKHEADERSIZE = 5,
        NET_PACKETHEADERSIZE = 3,
        NET_MAX_CLIENTS = 64,
        NET_MAX_CONSOLE_CLIENTS = 4,
        NET_MAX_SEQUENCE = 1 << 10,
        NET_SEQUENCE_MASK = NET_MAX_SEQUENCE - 1,

        NET_CONNSTATE_OFFLINE = 0,
        NET_CONNSTATE_CONNECT = 1,
        NET_CONNSTATE_PENDING = 2,
        NET_CONNSTATE_ONLINE = 3,
        NET_CONNSTATE_ERROR = 4,

        NET_PACKETFLAG_CONTROL = 1,
        NET_PACKETFLAG_CONNLESS = 2,
        NET_PACKETFLAG_RESEND = 4,
        NET_PACKETFLAG_COMPRESSION = 8,

        NET_CHUNKFLAG_VITAL = 1,
        NET_CHUNKFLAG_RESEND = 2,

        NET_CTRLMSG_KEEPALIVE = 0,
        NET_CTRLMSG_CONNECT = 1,
        NET_CTRLMSG_CONNECTACCEPT = 2,
        NET_CTRLMSG_ACCEPT = 3,
        NET_CTRLMSG_CLOSE = 4,

        NET_CONN_BUFFERSIZE = 1024 * 32,

        NET_ENUM_TERMINATOR,

        NETMSG_NULL = 0,

        // the first thing sent by the client
        // contains the version info for the client
        NETMSG_INFO = 1,

        // sent by server
        NETMSG_MAP_CHANGE,      // sent when client should switch map
        NETMSG_MAP_DATA,        // map transfer, contains a chunk of the map file
        NETMSG_CON_READY,       // connection is ready, client should send start info
        NETMSG_SNAP,            // normal snapshot, multiple parts
        NETMSG_SNAPEMPTY,       // empty snapshot
        NETMSG_SNAPSINGLE,      // ?
        NETMSG_SNAPSMALL,       //
        NETMSG_INPUTTIMING,     // reports how off the input was
        NETMSG_RCON_AUTH_STATUS,// result of the authentication
        NETMSG_RCON_LINE,       // line that should be printed to the remote console

        NETMSG_AUTH_CHALLANGE,  //
        NETMSG_AUTH_RESULT,     //

        // sent by client
        NETMSG_READY,           //
        NETMSG_ENTERGAME,
        NETMSG_INPUT,           // contains the inputdata from the client
        NETMSG_RCON_CMD,        //
        NETMSG_RCON_AUTH,       //
        NETMSG_REQUEST_MAP_DATA,//

        NETMSG_AUTH_START,      //
        NETMSG_AUTH_RESPONSE,   //

        // sent by both
        NETMSG_PING,
        NETMSG_PING_REPLY,
        NETMSG_ERROR,

        // sent by server (todo: move it up)
        NETMSG_RCON_CMD_ADD,
        NETMSG_RCON_CMD_REM,
    }

    public class CNetChunk
    {
        public int m_ClientID;
        public NETADDR m_Address; // only used when client_id == -1
        public int m_Flags;
        public int m_DataSize;
        public byte[] m_pData;
    }

    public class CNetChunkResend
    {
        public int m_Flags;
        public int m_DataSize;
        public byte[] m_pData;

        public int m_Sequence;
        public long m_LastSendTime;
        public long m_FirstSendTime;
    }

    public class CNetPacketConstruct
    {
        public int m_Flags;
        public int m_Ack;
        public int m_NumChunks;
        public int m_DataSize;
        public byte[] m_aChunkData;

        public CNetPacketConstruct()
        {
            m_aChunkData = new byte[(int)NetworkConsts.NET_MAX_PAYLOAD];
        }
    }

    public class CNetChunkHeader
    {
        public int m_Flags;
        public int m_Size;
        public int m_Sequence;

        public int Pack(byte[] pData, int pDataIndex)
        {
            pData[pDataIndex+0] = (byte) (((m_Flags & 3) << 6) | ((m_Size >> 4) & 0x3f));
            pData[pDataIndex + 1] = (byte) (m_Size & 0xf);

            if ((m_Flags & (int)NetworkConsts.NET_CHUNKFLAG_VITAL) != 0)
            {
                pData[pDataIndex + 1] = (byte) (pData[pDataIndex + 1] | ((m_Sequence >> 2) & 0xf0));
                pData[pDataIndex + 2] = (byte) (m_Sequence & 0xff);

                return pDataIndex +3;
            }
            return pDataIndex + 2;
        }

        public int Unpack(byte[] pData, int pDataIndex)
        {
            m_Flags = (pData[pDataIndex+0] >> 6) & 3;
            m_Size = ((pData[pDataIndex + 0] & 0x3f) << 4) | (pData[pDataIndex + 1] & 0xf);
            m_Sequence = -1;
            if ((m_Flags & (int)NetworkConsts.NET_CHUNKFLAG_VITAL) != 0)
            {
                m_Sequence = ((pData[pDataIndex + 1] & 0xf0) << 2) | pData[pDataIndex + 2];
                return pDataIndex + 3;
            }
            return pDataIndex + 2;
        }
    }

    public static class CNetBase
    {
        static readonly uint[] gs_aFreqTable = new uint[] {
            1<<30,4545,2657,431,1950,919,444,482,2244,617,838,542,715,1814,304,240,754,212,647,186,
            283,131,146,166,543,164,167,136,179,859,363,113,157,154,204,108,137,180,202,176,
            872,404,168,134,151,111,113,109,120,126,129,100,41,20,16,22,18,18,17,19,
            16,37,13,21,362,166,99,78,95,88,81,70,83,284,91,187,77,68,52,68,
            59,66,61,638,71,157,50,46,69,43,11,24,13,19,10,12,12,20,14,9,
            20,20,10,10,15,15,12,12,7,19,15,14,13,18,35,19,17,14,8,5,
            15,17,9,15,14,18,8,10,2173,134,157,68,188,60,170,60,194,62,175,71,
            148,67,167,78,211,67,156,69,1674,90,174,53,147,89,181,51,174,63,163,80,
            167,94,128,122,223,153,218,77,200,110,190,73,174,69,145,66,277,143,141,60,
            136,53,180,57,142,57,158,61,166,112,152,92,26,22,21,28,20,26,30,21,
            32,27,20,17,23,21,30,22,22,21,27,25,17,27,23,18,39,26,15,21,
            12,18,18,27,20,18,15,19,11,17,33,12,18,15,19,18,16,26,17,18,
            9,10,25,22,22,17,20,16,6,16,15,20,14,18,24,335,1517
        };
        private static readonly CHuffman ms_Huffman;

        static CNetBase()
        {
            ms_Huffman = new CHuffman();
            ms_Huffman.Init(gs_aFreqTable);
        }

        public static void SendControlMsg(NETSOCKET Socket, NETADDR pAddr, int Ack, 
            byte ControlMsg, string pExtraString)
        {
            CNetPacketConstruct Construct = new CNetPacketConstruct();
            var strBytes = Encoding.UTF8.GetBytes(pExtraString);

            Array.Copy(strBytes, 0, Construct.m_aChunkData, 1, strBytes.Length);

            Construct.m_DataSize = 1 + strBytes.Length;
            Construct.m_aChunkData[0] = ControlMsg;
            Construct.m_Flags = (int)NetworkConsts.NET_PACKETFLAG_CONTROL;
            Construct.m_Ack = Ack;
            Construct.m_NumChunks = 0;

            // send the control message
            SendPacket(Socket, pAddr, Construct);
        }

        public static void SendPacket(NETSOCKET Socket, NETADDR pAddr, CNetPacketConstruct pPacket)
        {
            byte[] aBuffer = new byte[(int)NetworkConsts.NET_MAX_PACKETSIZE];
            int CompressedSize = -1;
            int FinalSize = -1;

            // compress
            CompressedSize = ms_Huffman.Compress(pPacket.m_aChunkData, 0, pPacket.m_DataSize, aBuffer, 3, (int)NetworkConsts.NET_MAX_PACKETSIZE - 4);

            // check if the compression was enabled, successful and good enough
            if (CompressedSize > 0 && CompressedSize < pPacket.m_DataSize)
            {
                FinalSize = CompressedSize;
                pPacket.m_Flags |= (int)NetworkConsts.NET_PACKETFLAG_COMPRESSION;
            }
            else
            {
                // use uncompressed data
                FinalSize = pPacket.m_DataSize;
                Array.Copy(pPacket.m_aChunkData, 0, aBuffer, 3, pPacket.m_DataSize);
                pPacket.m_Flags &= ~(int)NetworkConsts.NET_PACKETFLAG_COMPRESSION;
            }

            // set header and send the packet if all things are good
            if (FinalSize >= 0)
            {
                FinalSize += (int)NetworkConsts.NET_PACKETHEADERSIZE;
                aBuffer[0] = (byte) (((pPacket.m_Flags << 4) & 0xf0) | ((pPacket.m_Ack >> 8) & 0xf));
                aBuffer[1] = (byte) (pPacket.m_Ack & 0xff);
                aBuffer[2] = (byte) pPacket.m_NumChunks;
                CSystem.net_udp_send(Socket, pAddr, aBuffer, FinalSize);

                //CSystem.dbg_msg("server", "FinalSize = {0}", FinalSize);
                //CSystem.dbg_msg("server", "m_Construct.m_Ack = {0}", pPacket.m_Ack);
                //CSystem.dbg_msg("server", "m_Construct.m_NumChunks = {0}", pPacket.m_NumChunks);
                //CSystem.dbg_msg("server", "m_Construct.m_Flags = {0}", pPacket.m_Flags);
            }
        }

        public static void SendPacketConnless(NETSOCKET Socket, NETADDR pAddr, byte[] pData, int DataSize)
        {
            byte[] aBuffer = new byte[(int)NetworkConsts.NET_MAX_PACKETSIZE];
            aBuffer[0] = 255;
            aBuffer[1] = 255;
            aBuffer[2] = 255;
            aBuffer[3] = 255;
            aBuffer[4] = 255;
            aBuffer[5] = 255;

            Array.Copy(pData, 0, aBuffer, 6, DataSize);
            CSystem.net_udp_send(Socket, pAddr, aBuffer, (int)(6 + DataSize));
        }

        public static bool UnpackPacket(byte[] pBuffer, int Size, CNetPacketConstruct pPacket)
        {
            // check the size
            if (Size < (int)NetworkConsts.NET_PACKETHEADERSIZE || Size > (int)NetworkConsts.NET_MAX_PACKETSIZE)
            {
                CSystem.dbg_msg("", "packet too small, {0}", Size);
                return false;
            }

            // read the packet
            pPacket.m_Flags = pBuffer[0] >> 4;
            pPacket.m_Ack = ((pBuffer[0] & 0xf) << 8) | pBuffer[1];
            pPacket.m_NumChunks = pBuffer[2];
            pPacket.m_DataSize = Size - (int)NetworkConsts.NET_PACKETHEADERSIZE;

            if ((pPacket.m_Flags & (int)NetworkConsts.NET_PACKETFLAG_CONNLESS) != 0)
            {
                if (Size < 6)
                {
                    //CSystem.dbg_msg("", "connection less packet too small, {0}", Size);
                    return false;
                }

                pPacket.m_Flags = (int)NetworkConsts.NET_PACKETFLAG_CONNLESS;
                pPacket.m_Ack = 0;
                pPacket.m_NumChunks = 0;
                pPacket.m_DataSize = Size - 6;
                pPacket.m_aChunkData = new byte[pPacket.m_DataSize];
                Array.Copy(pBuffer, 6, pPacket.m_aChunkData, 0, pPacket.m_DataSize);
            }
            else
            {
                pPacket.m_aChunkData = new byte[(int)NetworkConsts.NET_MAX_PAYLOAD];
                if ((pPacket.m_Flags & (int) NetworkConsts.NET_PACKETFLAG_COMPRESSION) != 0)
                    pPacket.m_DataSize = ms_Huffman.Decompress(pBuffer, 3, pPacket.m_DataSize, 
                        pPacket.m_aChunkData, 0, (int) NetworkConsts.NET_MAX_PAYLOAD);
                else
                {
                    pPacket.m_aChunkData = new byte[pPacket.m_DataSize];
                    Array.Copy(pBuffer, 3, pPacket.m_aChunkData, 0, pPacket.m_DataSize);
                }
            }

            // check for errors
            if (pPacket.m_DataSize < 0)
            {
                CSystem.dbg_msg("network", "error during packet decoding");
                return false;
            }

            // return success
            return true;
        }
        
        // The backroom is ack-NET_MAX_SEQUENCE/2. Used for knowing if we acked a packet or not
        public static bool IsSeqInBackroom(int Seq, int Ack)
        {
            int Bottom = (Ack - (int)NetworkConsts.NET_MAX_SEQUENCE / 2);
            if (Bottom < 0)
            {
                if (Seq <= Ack)
                    return true;
                if (Seq >= (Bottom + (int)NetworkConsts.NET_MAX_SEQUENCE))
                    return true;
            }
            else
            {
                if (Seq <= Ack && Seq >= Bottom)
                    return true;
            }

            return false;
        }
    }
    
    public class CNetRecvUnpacker
    {
        public byte[] m_aBuffer;
        public bool m_Valid;
        public CNetPacketConstruct m_Data;
        public CNetConnection m_pConnection;
        public int m_CurrentChunk;
        public int m_ClientID;
        public NETADDR m_Addr;

        private readonly CConfiguration g_Config;

        public CNetRecvUnpacker()
        {
            g_Config = CConfiguration.Instance;
            m_Data = new CNetPacketConstruct();
            m_aBuffer = new byte[(int)NetworkConsts.NET_MAX_PACKETSIZE];
            Clear();
        }

        public void Clear()
        {
            m_Valid = false;
        }

        public void Start(NETADDR pAddr, CNetConnection pConnection, int ClientID)
        {
            m_Addr = pAddr;
            m_pConnection = pConnection;
            m_ClientID = ClientID;
            m_CurrentChunk = 0;
            m_Valid = true;
        }

        public bool FetchChunk(CNetChunk pChunk)
        {
            CNetChunkHeader Header = new CNetChunkHeader();
            int pEnd = m_Data.m_DataSize;

            while (true)
            {
                // check for old data to unpack
                if (!m_Valid || m_CurrentChunk >= m_Data.m_NumChunks)
                {
                    Clear();
                    return false;
                }

                int pDataIndex = 0;
                // TODO: add checking here so we don't read too far
                for (int i = 0; i < m_CurrentChunk; i++)
                {
                    pDataIndex = Header.Unpack(m_Data.m_aChunkData, pDataIndex);
                    pDataIndex += Header.m_Size;
                }

                // unpack the header
                pDataIndex = Header.Unpack(m_Data.m_aChunkData, pDataIndex);
                m_CurrentChunk++;

                if (pDataIndex + Header.m_Size > pEnd)
                {
                    Clear();
                    return false;
                }

                // handle sequence stuff
                if (m_pConnection != null && (Header.m_Flags & (int)NetworkConsts.NET_CHUNKFLAG_VITAL) != 0)
                {
                    if (Header.m_Sequence == (m_pConnection.m_Ack + 1) % (int)NetworkConsts.NET_MAX_SEQUENCE)
                    {
                        // in sequence
                        m_pConnection.m_Ack = (ushort)((m_pConnection.m_Ack + 1) % (int)NetworkConsts.NET_MAX_SEQUENCE);
                    }
                    else
                    {
                        // old packet that we already got
                        if (CNetBase.IsSeqInBackroom(Header.m_Sequence, m_pConnection.m_Ack))
                            continue;

                        // out of sequence, request resend
                        if (g_Config.GetInt("Debug") != 0)
                            CSystem.dbg_msg("conn", "asking for resend {0} {1}", Header.m_Sequence,
                                (m_pConnection.m_Ack + 1) % (int)NetworkConsts.NET_MAX_SEQUENCE);
                        m_pConnection.SignalResend();
                        continue; // take the next chunk in the packet
                    }
                }

                // fill in the info
                pChunk.m_ClientID = m_ClientID;
                pChunk.m_Address = m_Addr;
                pChunk.m_Flags = 0;
                pChunk.m_DataSize = Header.m_Size;
                pChunk.m_pData = new byte[m_Data.m_aChunkData.Length - pDataIndex];
                Array.Copy(m_Data.m_aChunkData, pDataIndex, pChunk.m_pData, 0, pChunk.m_pData.Length);
                return true;
            }
        }
    }
}

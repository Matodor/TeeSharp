using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TeeSharp
{
    public enum ControlMessage
    {
        KEEPALIVE = 0,
        CONNECT = 1,
        CONNECTACCEPT = 2,
        ACCEPT = 3,
        CLOSE = 4,
    }

    [Flags]
    public enum SendFlag
    {
        VITAL = 1,
        CONNLESS = 2,
        FLUSH = 4,
    }

    [Flags]
    public enum ChunkFlags
    {
        VITAL = 1,
        RESEND = 2,
    }

    [Flags]
    public enum PacketFlag
    {
        CONTROL = 1,
        CONNLESS = 2,
        RESEND = 4,
        COMPRESSION = 8,
    }

    public enum ConnectionState
    {
        OFFLINE = 0,
        CONNECT,
        PENDING,
        ONLINE,
        ERROR,
    }

    public partial class Consts
    {
        public const int
            NET_MAX_PACKETSIZE = 1400,
            NET_MAX_PAYLOAD = NET_MAX_PACKETSIZE - 6,
            NET_MAX_CHUNKHEADERSIZE = 5,
            NET_PACKETHEADERSIZE = 3,

            NET_MAX_CLIENTS = 64,
            NET_MAX_SEQUENCE = 1024,

            NET_CONN_BUFFERSIZE = 1024 * 32;
    }

    public static class NetworkBase
    {
        private static readonly uint[] _freqTable = new uint[] {
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

        private static readonly Configuration _config;
        private static readonly Huffman _huffman;

        static NetworkBase()
        {
            _huffman = new Huffman();
            _huffman.Init(_freqTable);
            _config = Kernel.Get<Configuration>();
        }

        public static void SendControlMsg(UdpClient client, IPEndPoint addr, int ack, ControlMessage controlMsg,
            string extra)
        {
            var packet = new NetPacketConstruct
            {
                Flags = PacketFlag.CONTROL,
                Ack = ack,
                NumChunks = 0,
            };

            if (!string.IsNullOrEmpty(extra))
            {
                var bytes = Encoding.UTF8.GetBytes(extra);
                packet.DataSize = 1 + bytes.Length + 1;
                packet.ChunkData = new byte[packet.DataSize];
                Array.Copy(bytes, 0, packet.ChunkData, 1, bytes.Length);
            }
            else
            {
                packet.DataSize = 1;
                packet.ChunkData = new byte[1];
            }

            packet.ChunkData[0] = (byte) controlMsg;
        }

        public static void SendPacketConnless(UdpClient client, IPEndPoint addr, byte[] data, int dataSize)
        {
            var buffer = new byte[dataSize + 6];
            buffer[0] = 255;
            buffer[1] = 255;
            buffer[2] = 255;
            buffer[3] = 255;
            buffer[4] = 255;
            buffer[5] = 255;

            Array.Copy(data, 0, buffer, 6, dataSize);
            Base.SendUdp(client, addr, buffer, buffer.Length);
        }
        
        public static void SendPacket(UdpClient client, IPEndPoint addr, NetPacketConstruct packet)
        {
            var buffer = new byte[Consts.NET_MAX_PACKETSIZE];
            var compressedSize = _huffman.Compress(packet.ChunkData, 0, packet.DataSize,
                buffer, 3, buffer.Length - 4);
            var finalSize = 0;

            if (compressedSize > 0 && compressedSize < packet.DataSize)
            {
                finalSize = compressedSize;
                packet.Flags |= PacketFlag.COMPRESSION;
            }
            else
            {
                finalSize = packet.DataSize;
                Array.Copy(packet.ChunkData, 0, buffer, 3, packet.DataSize);
                packet.Flags &= ~PacketFlag.COMPRESSION;
            }

            if (finalSize >= 0)
            {
                finalSize += Consts.NET_PACKETHEADERSIZE;
                buffer[0] = (byte)((((int) packet.Flags << 4) & 0xf0) | ((packet.Ack >> 8) & 0xf));
                buffer[1] = (byte) (packet.Ack & 0xff);
                buffer[2] = (byte) packet.NumChunks;
                Base.SendUdp(client, addr, buffer, finalSize);
            }
        }

        public static bool UnpackPacket(byte[] data, int size, NetPacketConstruct packet)
        {
            if (size < Consts.NET_PACKETHEADERSIZE || size > Consts.NET_MAX_PACKETSIZE)
            {
                Base.DbgMessage("network", $"packet too small, {size} bytes");
                return false;
            }

            // read the packet
            packet.Flags = (PacketFlag) (data[0] >> 4);
            packet.Ack = ((data[0] & 0xf) << 8) | data[1]; 
            packet.NumChunks = data[2];
            packet.DataSize = size - Consts.NET_PACKETHEADERSIZE;

            if ((packet.Flags & PacketFlag.CONNLESS) != 0)
            {
                const int DATA_OFFSET = 6;
                if (size < DATA_OFFSET)
                {
                    Base.DbgMessage("network", $"connection less packet too small, {size} bytes");
                    return false;
                }

                packet.Flags = PacketFlag.CONNLESS;
                packet.Ack = 0;
                packet.NumChunks = 0;
                packet.DataSize = size - DATA_OFFSET;
                packet.ChunkData = new byte[packet.DataSize];

                Array.Copy(data, DATA_OFFSET, packet.ChunkData, 0, packet.DataSize);
            }
            else
            {
                if ((packet.Flags & PacketFlag.COMPRESSION) != 0)
                {
                    // Don't allow compressed control packets.
                    if ((packet.Flags & PacketFlag.CONTROL) != 0)
                        return false;

                    packet.DataSize = _huffman.Decompress(data, 3, packet.DataSize,
                        packet.ChunkData, 0, Consts.NET_MAX_PAYLOAD);
                }
                else
                {
                    packet.ChunkData = new byte[packet.DataSize];
                    Array.Copy(data, 3, packet.ChunkData, 0, packet.DataSize);
                }
            }

            // check for errors
            if (packet.DataSize < 0)
            {
                if (_config.GetInt("Debug") != 0)
                    Base.DbgMessage("network", "error during packet decoding");
                return false;
            }

            return true;
        }

        // The backroom is ack-NET_MAX_SEQUENCE/2. Used for knowing if we acked a packet or not
        public static bool IsSeqInBackroom(int seq, int ack)
        {
            var bottom = (ack - Consts.NET_MAX_SEQUENCE / 2);
            if (bottom < 0)
            {
                if (seq <= ack)
                    return true;
                if (seq >= (bottom + Consts.NET_MAX_SEQUENCE))
                    return true;
            }
            else
            {
                if (seq <= ack && seq >= bottom)
                    return true;
            }

            return false;
        }
    }
}

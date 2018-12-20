using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TeeSharp.Common;
using TeeSharp.Core;
using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public static class NetworkHelper
    {
        public const int ConnectionBufferSize = 1024 * 32;
        public const int MaxClients = 64;

        public const int PacketVersion = 1;
        public const int PacketHeaderSize = 7;
        public const int PacketHeaderSizeConnless = PacketHeaderSize + 2;

        public const int MaxPacketHeaderSize = PacketHeaderSizeConnless;
        public const int MaxPacketSize = 1400;
        public const int MaxPacketChunks = 256;
        public const int MaxChunkHeaderSize = 3;
        public const int MaxPayload = MaxPacketSize - MaxPacketHeaderSize;
        public const int MaxSequence = 1024;

        public const int SeedTime = 16;
        public const int AddrMaxStringSize = 1 + (8 * 4 + 7) + 1 + 1 + 5 + 1; // [XXXX:XXXX:XXXX:XXXX:XXXX:XXXX:XXXX:XXXX]:XXXXX

        public static readonly Huffman Huffman;

        static NetworkHelper()
        {
            Huffman = new Huffman(_freqTable);
        }

        public static uint Hash(byte[] data)
        {
            var hash = Secure.MD5.ComputeHash(data); // 16 bytes
            var bytes = new ReadOnlySpan<byte>(hash);
            var digest = new[]
            {
                BitConverter.ToUInt32(bytes),
                BitConverter.ToUInt32(bytes.Slice(4, 4)),
                BitConverter.ToUInt32(bytes.Slice(8, 4)),
                BitConverter.ToUInt32(bytes.Slice(12,4)),
            };

            return digest[0] ^ digest[1] ^ digest[2] ^ digest[3];
        }

        public static void SendConnectionMsgWithToken(UdpClient client, IPEndPoint endPoint,
            uint token, int ack, ConnectionMessages msg, uint myToken, bool extended)
        {
            Debug.Assert((token & ~TokenHelper.TokenMask) == 0, "token out of range");
            Debug.Assert((myToken & ~TokenHelper.TokenMask) == 0, "resp token out of range");

            var buffer = new byte[TokenHelper.TokenRequestDataSize];
            buffer[0] = (byte) ((myToken >> 24) & 0xff);
            buffer[1] = (byte) ((myToken >> 16) & 0xff);
            buffer[2] = (byte) ((myToken >> 8) & 0xff);
            buffer[3] = (byte) ((myToken & 0xff));

            SendConnectionMsg(client, endPoint, token, 0, msg, buffer, extended ? buffer.Length : 4);
        }

        public static void SendConnectionMsg(UdpClient client, IPEndPoint endPoint,
            uint token, int ack, ConnectionMessages msg, string extra)
        {
            if (string.IsNullOrWhiteSpace(extra))
            {
                SendConnectionMsg(client, endPoint, token, ack, msg, null, 0);
            }
            else
            {
                var data = Encoding.UTF8.GetBytes(extra);
                SendConnectionMsg(client, endPoint, token, ack, msg, data, data.Length);
            }
        }

        public static void SendConnectionMsg(UdpClient client, IPEndPoint endPoint,
            uint token, int ack, ConnectionMessages msg, byte[] extraData, int extraSize)
        {
            var packet = new NetworkChunkConstruct(1 + extraSize)
            {
                Token = token,
                Flags = PacketFlags.Control,
                Ack = ack,
                NumChunks = 0,
                DataSize = 1 + extraSize
            };
            packet.Data[0] = (byte) msg;

            if (extraSize > 0)
                Buffer.BlockCopy(extraData, 0, packet.Data, 1, extraSize);

            SendPacket(client, endPoint, packet);
        }

        public static void SendPacketConnless(UdpClient client, IPEndPoint endPoint,
            uint token, uint responseToken, byte[] data, int dataSize)
        {
            Debug.Assert(dataSize <= MaxPayload, "packet data size too high");
            Debug.Assert((token & ~TokenHelper.TokenMask) == 0, "token out of range");
            Debug.Assert((responseToken & ~TokenHelper.TokenMask) == 0, "resp token out of range");

            var buffer = new byte[PacketHeaderSizeConnless + dataSize];
            buffer[0] = (((int) PacketFlags.Connless << 2) & 0xfc) | (PacketVersion & 0x03);
            buffer[1] = (byte) ((token >> 24) & 0xff);
            buffer[2] = (byte) ((token >> 16) & 0xff);
            buffer[3] = (byte) ((token >> 8) & 0xff);
            buffer[4] = (byte) ((token) & 0xff);
            buffer[5] = (byte) ((responseToken >> 24) & 0xff);
            buffer[6] = (byte) ((responseToken >> 16) & 0xff);
            buffer[7] = (byte) ((responseToken >> 8) & 0xff);
            buffer[8] = (byte) ((responseToken) & 0xff);

            Buffer.BlockCopy(data, 0, buffer, PacketHeaderSizeConnless, dataSize);
            client.Send(buffer, buffer.Length, endPoint);
        }

        public static bool UnpackPacket(byte[] data, int dataLength, 
            NetworkChunkConstruct chunkConstruct)
        {
            throw new NotImplementedException();
        }

        public static void SendPacket(UdpClient client, IPEndPoint endPoint, 
            NetworkChunkConstruct packet)
        {
            if (packet.DataSize == 0)
                return;

            Debug.Assert((packet.Token & ~TokenHelper.TokenMask) == 0, "token out of range");

            var buffer = new byte[MaxPacketSize];
            var compressedSize = -1;

            if (!packet.Flags.HasFlag(PacketFlags.Control))
            {
                compressedSize = Huffman.Compress(packet.Data, 0, packet.DataSize,
                    buffer, PacketHeaderSize, MaxPayload);
            }

            int finalSize;
            if (compressedSize > 0 && compressedSize < packet.DataSize)
            {
                finalSize = compressedSize;
                packet.Flags |= PacketFlags.Compression;
            }
            else
            {
                finalSize = packet.DataSize;
                Buffer.BlockCopy(packet.Data, 0, buffer, PacketHeaderSize, packet.DataSize);
                packet.Flags &= ~PacketFlags.Compression;
            }

            finalSize += PacketHeaderSize;
            buffer[0] = (byte) ((((int) packet.Flags << 2) & 0xfc) | ((packet.Ack >> 8) & 0x03)); // flags and ack
            buffer[1] = (byte) ((packet.Ack) & 0xff); 
            buffer[2] = (byte) ((packet.NumChunks) & 0xff); 
            buffer[3] = (byte) ((packet.Token >> 24) & 0xff);
            buffer[4] = (byte) ((packet.Token >> 16) & 0xff);
            buffer[5] = (byte) ((packet.Token >> 8) & 0xff);
            buffer[6] = (byte) ((packet.Token) & 0xff);

            client.Send(buffer, finalSize, endPoint);
        }

        public static bool UdpClient(IPEndPoint endPoint, out UdpClient client)
        {
            try
            {
                client = new UdpClient(endPoint)
                {
                    Client =
                    {
                        Blocking = false,
                    }
                };
                return true;
            }
            catch (Exception e)
            {
                Debug.Exception("network", e.ToString());
                client = null;
                return false;
            }
        }

        public static bool IsSequenceInBackroom(int sequence, int ack)
        {
            var bottom = ack - MaxSequence / 2;
            if (bottom < 0)
            {
                if (sequence <= ack)
                    return true;
                if (sequence >= (bottom + MaxSequence))
                    return true;
            }
            else if (sequence <= ack && sequence >= bottom)
                return true;

            return false;
        }

        private static readonly int[] _freqTable = 
        {
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
    }
}
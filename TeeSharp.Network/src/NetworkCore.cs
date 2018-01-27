using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TeeSharp.Core;
using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public static class NetworkCore
    {
        public const int
            MAX_PACKET_SIZE = 1400,
            MAX_SEQUENCE = 1024,
            MAX_PAYLOAD = MAX_PACKET_SIZE - DATA_OFFSET,
            DATA_OFFSET = 6,
            PACKET_HEADER_SIZE = 3;

        private static readonly int[] _freqTable = {
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

        private static readonly Huffman _huffman;

        static NetworkCore()
        {
            _huffman = new Huffman(_freqTable);
        }

        public static bool CreateUdpClient(IPEndPoint endPoint, out UdpClient client)
        {
            try
            {
                client = new UdpClient(endPoint) {Client = {Blocking = false}};
                return true;
            }
            catch
            {
                client = null;
                return false;
            }
        }

        public static void SendControlMsg(UdpClient udpClient, IPEndPoint remote, 
            int ack, ConnectionMessages msg, string extra)
        {
            var packet = new NetworkChunkConstruct
            {
                Flags = PacketFlags.CONTROL,
                Ack = ack,
                NumChunks = 0,
            };

            if (string.IsNullOrWhiteSpace(extra))
            {
                packet.DataSize = 1;
                packet.Data = new byte[packet.DataSize];
            }
            else
            {
                var bytes = Encoding.UTF8.GetBytes(extra);
                packet.DataSize = 1 + bytes.Length;
                packet.Data = new byte[packet.DataSize];
                Buffer.BlockCopy(bytes, 0, packet.Data, 1, bytes.Length);
            }

            packet.Data[0] = (byte) msg;
            SendPacket(udpClient, remote, packet);
        }

        public static void SendPacket(UdpClient udpClient, IPEndPoint endPoint, 
            NetworkChunkConstruct packet)
        {
            var buffer = new byte[MAX_PACKET_SIZE];
            var compressedSize = _huffman.Compress(packet.Data, 0, packet.DataSize,
                buffer, PACKET_HEADER_SIZE, buffer.Length - PACKET_HEADER_SIZE);

            int finalSize;
            if (compressedSize > 0 && compressedSize < packet.DataSize)
            {
                finalSize = compressedSize;
                packet.Flags |= PacketFlags.COMPRESSION;
            }
            else
            {
                finalSize = packet.DataSize;
                Buffer.BlockCopy(packet.Data, 0, buffer, PACKET_HEADER_SIZE, packet.DataSize);
                packet.Flags &= ~PacketFlags.COMPRESSION;
            }

            finalSize += PACKET_HEADER_SIZE;
            buffer[0] = (byte) ((((int) packet.Flags << 4) & 0xf0) | ((packet.Ack >> 8) & 0xf));
            buffer[1] = (byte) (packet.Ack & 0xff);
            buffer[2] = (byte) packet.NumChunks;
            udpClient.Send(buffer, finalSize, endPoint);
        }

        /*
            Buffer.BlockCopy(shortSamples, 0, packetBytes, 0, shortSamples.Length * sizeof(short)).  
                -And the same trick works in reverse as well:
            Buffer.BlockCopy(packetBytes, readPosition, shortSamples, 0, payloadLength);
        */

        public static bool UnpackPacket(byte[] data, int size, NetworkChunkConstruct packet)
        {
            if (size < PACKET_HEADER_SIZE ||
                size > MAX_PACKET_SIZE)
            {
                Debug.Warning("network", $"packet too small, size={size}");
                return false;
            }

            packet.Flags = (PacketFlags) (data[0] >> 4);
            packet.Ack = ((data[0] & 0xf) << 8) | data[1];
            packet.NumChunks = data[2];
            packet.DataSize = size - PACKET_HEADER_SIZE;

            if (packet.Flags.HasFlag(PacketFlags.CONNLESS))
            {
                if (size < DATA_OFFSET)
                {
                    Debug.Warning("network", $"connection less packet too small, size={size}");
                    return false;
                }

                packet.Flags = PacketFlags.CONNLESS;
                packet.Ack = 0;
                packet.NumChunks = 0;
                packet.DataSize = size - DATA_OFFSET;
                packet.Data = new byte[packet.DataSize];

                Buffer.BlockCopy(data, DATA_OFFSET, packet.Data, 0, packet.DataSize);
            }
            else
            {
                if (packet.Flags.HasFlag(PacketFlags.COMPRESSION))
                {
                    if (packet.Flags.HasFlag(PacketFlags.CONTROL))
                        return false;

                    packet.Data = new byte[MAX_PAYLOAD];
                    packet.DataSize = _huffman.Decompress(data, PACKET_HEADER_SIZE, 
                        packet.DataSize, packet.Data, 0, MAX_PAYLOAD);
                }
                else
                {
                    packet.Data = new byte[packet.DataSize];
                    Buffer.BlockCopy(data, PACKET_HEADER_SIZE, packet.Data, 0, packet.DataSize);
                }
            }

            if (packet.DataSize < 0)
            {
                Debug.Warning("network", "error during packet decoding");
                return false;
            }

            return true;
        }

        public static bool CompareEndPoints(IPEndPoint first, IPEndPoint second, 
            bool comparePorts)
        {
            return first.Address.Equals(second.Address) && (!comparePorts || first.Port == second.Port);
        }

        public static void SendPacketConnless(UdpClient udpClient, IPEndPoint endPoint, 
            byte[] data, int dataSize)
        {
            var buffer = new byte[dataSize + DATA_OFFSET];
            buffer[0] = 255;
            buffer[1] = 255;
            buffer[2] = 255;
            buffer[3] = 255;
            buffer[4] = 255;
            buffer[5] = 255;

            Buffer.BlockCopy(data, 0, buffer, DATA_OFFSET, dataSize);
            udpClient.Send(buffer, buffer.Length, endPoint);
        }

        public static bool IsSeqInBackroom(int seq, int ack)
        {
            var bottom = ack - MAX_SEQUENCE / 2;
            if (bottom < 0)
            {
                if (seq <= ack)
                    return true;
                if (seq >= (bottom + MAX_SEQUENCE))
                    return true;
            }
            else if (seq <= ack && seq >= bottom)
                return true;

            return false;
        }
    }
}
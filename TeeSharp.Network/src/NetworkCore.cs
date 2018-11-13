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
            MAX_PAYLOAD = MAX_PACKET_SIZE - 10,
            MAX_CHUNK_HEADER_SIZE = 5,
            PACKET_HEADER_SIZE = 7,
            PACKET_HEADER_SIZE_WITHOUT_TOKEN = 3,
            MAX_CLIENTS = 16,
            MAX_CONSOLE_CLIENTS = 4,
            MAX_SEQUENCE = 1024,
            DATA_OFFSET = 6,
            COMPATIBILITY_SEQ = 2,
            SEQUENCE_MASK = MAX_SEQUENCE - 1;

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

        public static IPAddress GetLocalIP(AddressFamily addressFamily)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == addressFamily)
                {
                    return ip;
                }
            }

            throw new Exception($"No network adapters with an {addressFamily} address family in the system!");
        }

        public static bool CreateUdpClient(IPEndPoint endPoint, out UdpClient client)
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
                Debug.Exception("exception", e.ToString());
                client = null;
                return false;
            }
        }

        public static void SendControlMsg(UdpClient udpClient,
            IPEndPoint remote, int ack, bool useToken, uint token,
            ConnectionMessages msg, byte[] extra)
        {
            NetworkChunkConstruct packet;
            if (extra == null || extra.Length == 0)
            {
                packet = new NetworkChunkConstruct(1)
                {
                    DataSize = 1
                };
            }
            else
            {
                packet = new NetworkChunkConstruct(1 + extra.Length)
                {
                    DataSize = 1 + extra.Length
                };
                Buffer.BlockCopy(extra, 0, packet.Data, 1, extra.Length);
            }

            packet.NumChunks = 0;
            packet.Flags = PacketFlags.CONTROL | (useToken ? PacketFlags.TOKEN : PacketFlags.NONE);
            packet.Ack = ack;
            packet.Token = token;
            packet.Data[0] = (byte)msg;
            SendPacket(udpClient, remote, packet);
        }

        public static void SendControlMsg(UdpClient udpClient, 
            IPEndPoint remote, int ack, bool useToken, uint token,
            ConnectionMessages msg, string extra)
        {
            if (string.IsNullOrWhiteSpace(extra))
            {
                SendControlMsg(udpClient, remote, ack, useToken, token,
                    msg, (byte[]) null);
            }
            else
            {
                SendControlMsg(udpClient, remote, ack, useToken, token,
                    msg, Encoding.UTF8.GetBytes(extra));
            }
        }

        public static void SendPacket(UdpClient udpClient, IPEndPoint endPoint, 
            NetworkChunkConstruct packet)
        {
            var buffer = new byte[MAX_PACKET_SIZE];
            var headerSize = PACKET_HEADER_SIZE_WITHOUT_TOKEN;
            var compressedSize = -1;

            if (packet.Flags.HasFlag(PacketFlags.TOKEN))
            {
                headerSize = PACKET_HEADER_SIZE;
                packet.Token.ToByteArray(buffer, 3);
            }

            if (!packet.Flags.HasFlag(PacketFlags.CONTROL))
            {
                compressedSize = _huffman.Compress(packet.Data, 0, packet.DataSize,
                    buffer, headerSize, buffer.Length - headerSize);
            }

            //compressedSize = _huffman.Compress(packet.Data, 0, packet.DataSize,
            //    buffer, PACKET_HEADER_SIZE, buffer.Length - PACKET_HEADER_SIZE);
            
            int finalSize;
            if (compressedSize > 0 && compressedSize < packet.DataSize)
            {
                finalSize = compressedSize;
                packet.Flags |= PacketFlags.COMPRESSION;
            }
            else
            {
                finalSize = packet.DataSize;
                Buffer.BlockCopy(packet.Data, 0, buffer, headerSize, packet.DataSize);
                packet.Flags &= ~PacketFlags.COMPRESSION;
            }

            finalSize += headerSize;
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

        public static bool UnpackPacket(byte[] buffer, int size, 
            NetworkChunkConstruct packet)
        {
            if (size < PACKET_HEADER_SIZE_WITHOUT_TOKEN ||
                size > MAX_PACKET_SIZE)
            {
                Debug.Warning("network", $"packet too small, size={size}");
                return false;
            }
            
            packet.Flags = (PacketFlags) (buffer[0] >> 2);
            packet.Ack = ((buffer[0] & 0x3) << 8) | buffer[1];
            packet.NumChunks = buffer[2];
            packet.DataSize = size - PACKET_HEADER_SIZE_WITHOUT_TOKEN;
            packet.Token = 0;

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
                Buffer.BlockCopy(buffer, DATA_OFFSET, packet.Data, 0, packet.DataSize);
            }
            else
            {
                var dataStart = PACKET_HEADER_SIZE_WITHOUT_TOKEN;
                if (packet.Flags.HasFlag(PacketFlags.TOKEN))
                {
                    if (size < PACKET_HEADER_SIZE)
                    {
                        Debug.Warning("network", $"packet with token too small, {size}");
                        return false;
                    }

                    dataStart = PACKET_HEADER_SIZE;
                    packet.DataSize -= 4;
                    packet.Token = buffer.ToUInt32(3);
                }

                if (packet.Flags.HasFlag(PacketFlags.COMPRESSION))
                {
                    packet.DataSize = _huffman.Decompress(buffer, dataStart, packet.DataSize,
                        packet.Data, 0, MAX_PAYLOAD);
                }
                else
                {
                    Buffer.BlockCopy(buffer, dataStart, packet.Data, 0, packet.DataSize);
                }

                /*if (flags.HasFlag(PacketFlags.COMPRESSION))
                {
                    if (flags.HasFlag(PacketFlags.CONTROL))
                        return false;

                    dataSize = _huffman.Decompress(buffer, PACKET_HEADER_SIZE,
                        dataSize, packet.Data, 0, MAX_PAYLOAD);
                }
                else
                {
                    Buffer.BlockCopy(buffer, PACKET_HEADER_SIZE, packet.Data, 0, dataSize);
                }

                packet.Flags = flags;
                packet.Ack = ack;
                packet.NumChunks = numChunks;
                packet.DataSize = dataSize;*/
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
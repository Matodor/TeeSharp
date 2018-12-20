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
    }
}
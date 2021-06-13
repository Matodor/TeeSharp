using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TeeSharp.Core.Extensions;
using TeeSharp.Core.Helpers;

namespace TeeSharp.Network
{
    public static class NetworkHelper
    {
        public static bool TryUnpackPacket(
            Span<byte> data,
            NetworkPacket packet,
            ref bool isSixUp,
            ref SecurityToken securityToken,
            ref SecurityToken responseToken)
        {
            if (data.Length < NetworkConstants.PacketHeaderSize ||
                data.Length > NetworkConstants.MaxPacketSize)
            {
                return true;
            }

            packet.Flags = (PacketFlags) (data[0] >> 2);

            if (packet.Flags.HasFlag(PacketFlags.ConnectionLess))
            {
                isSixUp = (data[0] & 0b_0000_0011) == 0b_0000_0001;

                var dataStart = isSixUp ? 9 : NetworkConstants.PacketConnLessDataOffset;
                if (dataStart > data.Length)
                    return false;

                if (isSixUp)
                {
                    securityToken = data.Slice(1, 4).Deserialize<SecurityToken>();
                    responseToken = data.Slice(5, 4).Deserialize<SecurityToken>();
                }

                packet.Flags = PacketFlags.ConnectionLess;
                packet.Ack = 0;
                packet.ChunksCount = 0;
                packet.DataSize = data.Length - dataStart;
                
                data.Slice(dataStart, packet.DataSize)
                    .CopyTo(packet.Data);

                if (!isSixUp && data
                    .Slice(0, NetworkConstants.PacketHeaderExtended.Length)
                    .SequenceEqual(NetworkConstants.PacketHeaderExtended))
                {
                    packet.Flags |= PacketFlags.Extended;
                    data.Slice(NetworkConstants.PacketHeaderExtended.Length, packet.ExtraData.Length)
                        .CopyTo(packet.ExtraData);
                }
            }
            else
            {
                if (packet.Flags.HasFlag(PacketFlags.Unused))
                    isSixUp = true;

                var dataStart = isSixUp ? 7 : NetworkConstants.PacketHeaderSize;
                if (dataStart > data.Length)
                    return false;

                packet.Ack = ((data[0] & 0b_0000_0011) << 8) | data[1];
                packet.ChunksCount = data[2];
                packet.DataSize = data.Length - dataStart;

                if (isSixUp)
                {
                    packet.Flags = PacketFlags.None;
                    
                    var sixUpFlags = (PacketFlagsSixUp) packet.Flags;
                    if (sixUpFlags.HasFlag(PacketFlagsSixUp.Connection))
                        packet.Flags |= PacketFlags.ConnectionState;
                    if (sixUpFlags.HasFlag(PacketFlagsSixUp.Resend))
                        packet.Flags |= PacketFlags.Resend;
                    if (sixUpFlags.HasFlag(PacketFlagsSixUp.Compression))
                        packet.Flags |= PacketFlags.Compression;

                    securityToken = data.Slice(3, 4).Deserialize<SecurityToken>();
                }

                if (packet.Flags.HasFlag(PacketFlags.Compression))
                {
                    if (packet.Flags.HasFlag(PacketFlags.ConnectionState))
                        return false;

                    throw new NotImplementedException();
                }
                else
                {
                    data.Slice(dataStart, packet.DataSize).CopyTo(packet.Data);
                }
            }

            return packet.DataSize >= 0;
        }

        // ReSharper disable once InconsistentNaming
        public static bool TryGetLocalIP(out IPAddress ip)
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            socket.Connect("8.8.8.8", 65530);

            try
            {
                ip = ((IPEndPoint) socket.LocalEndPoint).Address;
                return true;
            }
            catch
            {
                ip = default;
                return false;
            }
        }
        
        // ReSharper disable once InconsistentNaming
        public static bool TryGetUdpClient(IPEndPoint localEP, out UdpClient client)
        {
            try
            {
                client = localEP == null 
                    ? new UdpClient() 
                    : new UdpClient(localEP);
                return true;
            }
            catch
            {
                client = null;
                return false;
            }
        }

        public static void SendPacket(UdpClient client, IPEndPoint endPoint,
            NetworkPacket packet, SecurityToken securityToken, bool isSixUp)
        {
            var buffer = new Span<byte>(new byte[NetworkConstants.MaxPacketSize]);
            var headerSize = NetworkConstants.PacketHeaderSize;

            if (isSixUp)
            {
                headerSize += TypeHelper<SecurityToken>.Size;
                securityToken.CopyTo(buffer.Slice(NetworkConstants.PacketHeaderSize));
            }
            else if (securityToken != SecurityToken.Unsupported)
            {
                securityToken.CopyTo(buffer.Slice(NetworkConstants.PacketHeaderSize));
                // asdasd
            }
        }
        
        public static void SendData(UdpClient client, IPEndPoint endPoint, 
            ReadOnlySpan<byte> data, 
            ReadOnlySpan<byte> extraData = default)
        {
            var bufferSize = NetworkConstants.PacketConnLessDataOffset + data.Length;
            if (bufferSize > NetworkConstants.MaxPacketSize)
                throw new Exception("Maximum packet size exceeded.");

            var buffer = new Span<byte>(new byte[bufferSize]);
            if (extraData.IsEmpty)
            {
                buffer
                    .Slice(0, NetworkConstants.PacketConnLessDataOffset)
                    .Fill(255);
            }
            else
            {
                NetworkConstants.PacketHeaderExtended.CopyTo(buffer);
                extraData
                    .Slice(0, NetworkConstants.PacketExtraDataSize)
                    .CopyTo(buffer.Slice(NetworkConstants.PacketHeaderExtended.Length));
            }
            
            data.CopyTo(buffer.Slice(NetworkConstants.PacketConnLessDataOffset));
            client.BeginSend(
                buffer.ToArray(), 
                buffer.Length,
                endPoint, 
                EndSendCallback, 
                client
            );
        }

        private static void EndSendCallback(IAsyncResult result)
        {
            var client = (UdpClient) result.AsyncState;
            client?.EndSend(result);
        }
        
        public static void SendConnStateMsg(UdpClient client, IPEndPoint endPoint, 
            ConnectionStateMsg state, SecurityToken token, int ack, 
            bool isSixUp, string extraMsg)
        {
            if (string.IsNullOrEmpty(extraMsg))
            {
                SendConnStateMsg(client, endPoint, state, token, ack, isSixUp, Span<byte>.Empty);
            }
            else
            {
                var bufferLen = Encoding.UTF8.GetMaxByteCount(extraMsg.Length);
                var buffer = new Span<byte>(new byte[bufferLen]);
                var length = Encoding.UTF8.GetBytes(extraMsg.AsSpan(), buffer);
                SendConnStateMsg(client, endPoint, state, token, ack, isSixUp, buffer.Slice(0, length));
            }
        }
        
        public static void SendConnStateMsg(UdpClient client, IPEndPoint endPoint, 
            ConnectionStateMsg state, SecurityToken token, int ack, 
            bool isSixUp, Span<byte> extraData)
        {
            var packet = new NetworkPacket
            {
                Flags = PacketFlags.ConnectionState,
                Ack = ack,
                ChunksCount = 0,
                DataSize = 1 + extraData.Length,
            };

            packet.Data = new byte[packet.DataSize];
            packet.Data[0] = (byte) state;

            if (!extraData.IsEmpty)
                extraData.CopyTo(packet.Data.AsSpan(1));
            
            // SendPacket(client, endPoint, packet);
        }
    }
}
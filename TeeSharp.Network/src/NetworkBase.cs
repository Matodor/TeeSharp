using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using TeeSharp.Core.Extensions;

namespace TeeSharp.Network
{
    public static class NetworkBase
    {
        public static bool TryUnpackPacket(
            Span<byte> data,
            NetworkChunks chunks,
            ref bool isSixUp,
            ref SecurityToken securityToken,
            ref SecurityToken responseToken)
        {
            if (data.Length < NetworkConstants.PacketHeaderSize ||
                data.Length > NetworkConstants.MaxPacketSize)
            {
                return true;
            }

            chunks.Flags = (ChunkFlags) (data[0] >> 2);

            if (chunks.Flags.HasFlag(ChunkFlags.ConnectionLess))
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

                chunks.Flags = ChunkFlags.ConnectionLess;
                chunks.Ack = 0;
                chunks.Count = 0;
                chunks.DataSize = data.Length - dataStart;
                data.Slice(dataStart, chunks.DataSize).CopyTo(chunks.Data);

                if (!isSixUp && data
                    .Slice(0, NetworkConstants.PacketHeaderExtended.Length)
                    .SequenceEqual(NetworkConstants.PacketHeaderExtended))
                {
                    chunks.Flags |= ChunkFlags.Extended;
                    data.Slice(NetworkConstants.PacketHeaderExtended.Length, chunks.ExtraData.Length)
                        .CopyTo(chunks.ExtraData);
                }
            }
            else
            {
                if (chunks.Flags.HasFlag(ChunkFlags.Unused))
                    isSixUp = true;

                var dataStart = isSixUp ? 7 : NetworkConstants.PacketHeaderSize;
                if (dataStart > data.Length)
                    return false;

                chunks.Ack = ((data[0] & 0b_0000_0011) << 8) | data[1];
                chunks.Count = data[2];
                chunks.DataSize = data.Length - dataStart;

                if (isSixUp)
                {
                    chunks.Flags = ChunkFlags.None;

                    if (((ChunkFlagsSixUp) chunks.Flags).HasFlag(ChunkFlagsSixUp.Control))
                        chunks.Flags |= ChunkFlags.Control;
                    if (((ChunkFlagsSixUp) chunks.Flags).HasFlag(ChunkFlagsSixUp.Resend))
                        chunks.Flags |= ChunkFlags.Resend;
                    if (((ChunkFlagsSixUp) chunks.Flags).HasFlag(ChunkFlagsSixUp.Compression))
                        chunks.Flags |= ChunkFlags.Compression;

                    securityToken = data.Slice(3, 4).Deserialize<SecurityToken>();
                }

                if (chunks.Flags.HasFlag(ChunkFlags.Compression))
                {
                    if (chunks.Flags.HasFlag(ChunkFlags.Control))
                        return false;
                    
                    // TODO decomprassion
                }
                else
                {
                    data.Slice(dataStart, chunks.DataSize).CopyTo(chunks.Data);
                }
            }

            return chunks.DataSize >= 0;
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
        
        public static void SendData(UdpClient client, IPEndPoint endPoint, ReadOnlySpan<byte> data)
        {
            var bufferSize = NetworkConstants.PacketConnLessDataOffset + data.Length;
            if (bufferSize > NetworkConstants.MaxPacketSize)
                throw new Exception("Maximum packet size exceeded.");

            var buffer = (Span<byte>) stackalloc byte[bufferSize];
            buffer.Slice(0, NetworkConstants.PacketConnLessDataOffset).Fill(255);
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
    }
}
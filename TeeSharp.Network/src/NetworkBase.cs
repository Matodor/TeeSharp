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
            ChunksData chunksData,
            ref bool isSixUp,
            ref SecurityToken securityToken,
            ref SecurityToken responseToken)
        {
            if (data.Length < NetworkConstants.PacketHeaderSize ||
                data.Length > NetworkConstants.MaxPacketSize)
            {
                return true;
            }

            chunksData.Flags = (ChunkFlags) (data[0] >> 2);

            if (chunksData.Flags.HasFlag(ChunkFlags.ConnectionLess))
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

                chunksData.Flags = ChunkFlags.ConnectionLess;
                chunksData.Ack = 0;
                chunksData.Count = 0;
                chunksData.DataSize = data.Length - dataStart;
                
                data.Slice(dataStart, chunksData.DataSize)
                    .CopyTo(chunksData.Data);

                if (!isSixUp && data
                    .Slice(0, NetworkConstants.PacketHeaderExtended.Length)
                    .SequenceEqual(NetworkConstants.PacketHeaderExtended))
                {
                    chunksData.Flags |= ChunkFlags.Extended;
                    data.Slice(NetworkConstants.PacketHeaderExtended.Length, chunksData.ExtraData.Length)
                        .CopyTo(chunksData.ExtraData);
                }
            }
            else
            {
                if (chunksData.Flags.HasFlag(ChunkFlags.Unused))
                    isSixUp = true;

                var dataStart = isSixUp ? 7 : NetworkConstants.PacketHeaderSize;
                if (dataStart > data.Length)
                    return false;

                chunksData.Ack = ((data[0] & 0b_0000_0011) << 8) | data[1];
                chunksData.Count = data[2];
                chunksData.DataSize = data.Length - dataStart;

                if (isSixUp)
                {
                    chunksData.Flags = ChunkFlags.None;

                    if (((ChunkFlagsSixUp) chunksData.Flags).HasFlag(ChunkFlagsSixUp.Control))
                        chunksData.Flags |= ChunkFlags.Control;
                    if (((ChunkFlagsSixUp) chunksData.Flags).HasFlag(ChunkFlagsSixUp.Resend))
                        chunksData.Flags |= ChunkFlags.Resend;
                    if (((ChunkFlagsSixUp) chunksData.Flags).HasFlag(ChunkFlagsSixUp.Compression))
                        chunksData.Flags |= ChunkFlags.Compression;

                    securityToken = data.Slice(3, 4).Deserialize<SecurityToken>();
                }

                if (chunksData.Flags.HasFlag(ChunkFlags.Compression))
                {
                    if (chunksData.Flags.HasFlag(ChunkFlags.Control))
                        return false;
                    
                    // TODO decomprassion
                }
                else
                {
                    data.Slice(dataStart, chunksData.DataSize).CopyTo(chunksData.Data);
                }
            }

            return chunksData.DataSize >= 0;
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
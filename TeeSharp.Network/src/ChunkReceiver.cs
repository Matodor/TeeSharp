using System;
using System.Net;
using TeeSharp.Core;
using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public class ChunkReceiver : BaseChunkReceiver
    {
        public ChunkReceiver()
        {
            ChunkConstruct = new ChunkConstruct(NetworkHelper.MaxPayload);
        }

        public override void Start(IPEndPoint remote, 
            BaseNetworkConnection connection, int clientId)
        {
            EndPoint = remote;
            Connection = connection;
            ClientId = clientId;
            CurrentChunk = 0;
            Valid = true;
        }

        public override void Clear()
        {
            Valid = false;
        }

        public override bool FetchChunk(ref Chunk packet)
        {
            var header = new ChunkHeader();
            var end = ChunkConstruct.DataSize;

            while (true)
            {
                if (!Valid || CurrentChunk >= ChunkConstruct.NumChunks)
                {
                    Clear();
                    return false;
                }

                var dataOffset = 0;
                for (var i = 0; i < CurrentChunk; i++)
                {
                    dataOffset = header.Unpack(ChunkConstruct.Data, dataOffset);
                    dataOffset += header.Size;
                }
                
                dataOffset = header.Unpack(ChunkConstruct.Data, dataOffset);
                CurrentChunk++;

                if (dataOffset + header.Size > end)
                {
                    Clear();
                    return false;
                }

                if (header.Flags.HasFlag(ChunkFlags.Vital))
                {
                    if (header.Sequence == (Connection.Ack + 1) % NetworkHelper.MaxSequence)
                    {
                        Connection.Ack = (Connection.Ack + 1) % NetworkHelper.MaxSequence;
                    }
                    else
                    {
                        if (NetworkHelper.IsSequenceInBackroom(header.Sequence, Connection.Ack))
                            continue;

                        Debug.Log("connection",
                            $"asking for resend {header.Sequence} {(Connection.Ack + 1) % NetworkHelper.MaxSequence}");
                        Connection.SignalResend();
                        continue;
                    }
                }

                packet = new Chunk
                {
                    ClientId = ClientId,
                    EndPoint = EndPoint,
                    Flags = header.Flags.HasFlag(ChunkFlags.Vital)
                        ? SendFlags.Vital 
                        : SendFlags.None,
                    DataSize = header.Size,
                    Data = new byte[header.Size]
                };

                Buffer.BlockCopy(ChunkConstruct.Data, dataOffset, packet.Data, 0, header.Size);
                return true;
            }
        }
    }
}
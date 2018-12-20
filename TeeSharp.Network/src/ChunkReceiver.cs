using System;
using System.Net;
using TeeSharp.Core;
using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public class ChunkReceiver : BaseChunkReceiver
    {
        protected override bool Valid { get; set; }
        protected override int CurrentChunk { get; set; }
        protected override int ClientId { get; set; }
        protected override BaseNetworkConnection Connection { get; set; }
        protected override IPEndPoint EndPoint { get; set; }

        public ChunkReceiver()
        {
            ChunkConstruct = new NetworkChunkConstruct();
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

        public override bool FetchChunk(ref NetworkChunk packet)
        {
            var header = new NetworkChunkHeader();
            var end = ChunkConstruct.DataSize;

            while (true)
            {
                if (!Valid || CurrentChunk >= ChunkConstruct.NumChunks)
                {
                    Clear();
                    packet = null;
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
                    packet = null;
                    return false;
                }

                if (Connection != null && header.Flags.HasFlag(ChunkFlags.VITAL))
                {
                    if (Connection.UnknownAck ||
                        header.Sequence == (Connection.Ack + 1) % NetworkCore.MAX_SEQUENCE)
                    {
                        Connection.UnknownAck = false;
                        Connection.Ack = (Connection.Ack + 1) % NetworkCore.MAX_SEQUENCE;
                    }
                    else
                    {
                        if (NetworkCore.IsSeqInBackroom(header.Sequence, Connection.Ack))
                            continue;

                        Debug.Log("connection",
                            $"asking for resend {header.Sequence} {(Connection.Ack + 1) % NetworkCore.MAX_SEQUENCE}");
                        Connection.SignalResend();
                        continue;
                    }
                }

                packet = new NetworkChunk
                {
                    ClientId = ClientId,
                    EndPoint = EndPoint,
                    Flags = (SendFlags) header.Flags,
                    DataSize = header.Size,
                    Data = new byte[header.Size]
                };

                Buffer.BlockCopy(ChunkConstruct.Data, dataOffset, packet.Data, 0, header.Size);
                return true;
            }
        }
    }
}
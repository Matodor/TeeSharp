using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TeeSharp.Core;
using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public class NetworkConnection : BaseNetworkConnection
    {
        public override NetworkConnectionConfig Config { get; set; }
        public override ConnectionState State { get; protected set; }
        public override long ConnectedAt { get; protected set; }
        public override IPEndPoint EndPoint { get; protected set; }
        public override string Error { get; protected set; }

        public override int Sequence { get; set; }
        public override int Ack { get; set; }

        protected override UdpClient UdpClient { get; set; }
        protected override long LastReceiveTime { get; set; }
        protected override long LastSendTime { get; set; }
        protected override int BufferSize { get; set; }
        protected override bool RemoteClosed { get; set; }
        protected override Queue<NetworkChunkResend> ResendQueue { get; set; }
        protected override NetworkChunkConstruct ResendQueueConstruct { get; set; }

        private const int SIZEOF_NETWORK_CHUNK_RESEND = 32;

        public NetworkConnection()
        {
            Error = string.Empty;
            ResendQueueConstruct = new NetworkChunkConstruct
            {
                Data = new byte[NetworkCore.MAX_PAYLOAD]
            };
            ResendQueue = new Queue<NetworkChunkResend>();
        }

        public override void ResetQueueConstruct()
        {
            ResendQueueConstruct.DataSize = 0;
            ResendQueueConstruct.Flags = PacketFlags.NONE;
            ResendQueueConstruct.NumChunks = 0;
            ResendQueueConstruct.Ack = 0;
        }

        public override void Reset()
        {
            State = ConnectionState.OFFLINE;
            ConnectedAt = 0;

            Ack = 0;
            Sequence = 0;
            RemoteClosed = false;
            ResendQueue.Clear();
            BufferSize = 0;

            LastReceiveTime = 0;
            LastSendTime = 0;

            Error = null;
            EndPoint = null;

            ResetQueueConstruct();
        }

        public override void Init(UdpClient udpClient, NetworkConnectionConfig config)
        {
            Config = config;
            UdpClient = udpClient;
            Reset();
        }

        public override void Update()
        {
            var now = Time.Get();

            if (State == ConnectionState.OFFLINE ||
                State == ConnectionState.ERROR)
            {
                return;
            }

            if (State != ConnectionState.OFFLINE &&
                State != ConnectionState.CONNECT &&
                (now - LastReceiveTime) > Time.Freq() * Config.ConnectionTimeout)
            {
                State = ConnectionState.ERROR;
                Error = "Timeout";
                return;
            }

            if (ResendQueue.Count > 0)
            {
                var resend = ResendQueue.Peek();
                if (now - resend.FirstSendTime > Time.Freq() * Config.ConnectionTimeout)
                {
                    State = ConnectionState.ERROR;
                    Error = $"Too weak connection (not acked for {Config.ConnectionTimeout} seconds)";
                }
                else if (now - resend.LastSendTime > Time.Freq())
                {
                    ResendChunk(resend);
                }
            }

            if (State == ConnectionState.ONLINE)
            {
                if (now - LastSendTime > Time.Freq() / 2)
                {
                    var flushedChunks = Flush();
                    if (flushedChunks != 0)
                        Debug.Log("connection", $"flushed connection due to timeout. {flushedChunks} chunks.");
                }

                if (now - LastSendTime > Time.Freq())
                    SendControlMsg(ConnectionMessages.KEEPALIVE, "");
            }
            else if (State == ConnectionState.CONNECT)
            {
                if (now - LastSendTime > Time.Freq() / 2)
                    SendControlMsg(ConnectionMessages.CONNECT, "");
            }
            else if (State == ConnectionState.PENDING)
            {
                if (now - LastSendTime > Time.Freq() / 2)
                    SendControlMsg(ConnectionMessages.CONNECTACCEPT, "");
            }
        }

        public override void Disconnect(string reason)
        {
            if (State == ConnectionState.OFFLINE)
                return;

            if (!RemoteClosed)
            {
                SendControlMsg(ConnectionMessages.CLOSE, reason);

                if (!string.IsNullOrWhiteSpace(reason))
                    Error = reason;
            }

            Reset();
        }

        public override bool Feed(NetworkChunkConstruct packet, IPEndPoint remote)
        {
            var now = Time.Get();

            if (packet.Flags.HasFlag(PacketFlags.RESEND))
                Resend();

            if (packet.Flags.HasFlag(PacketFlags.CONTROL))
            {
                var msg = (ConnectionMessages) packet.Data[0];
                if (msg == ConnectionMessages.CLOSE)
                {
                    if (!NetworkCore.CompareEndPoints(EndPoint, remote, true))
                        return false;

                    State = ConnectionState.ERROR;
                    RemoteClosed = true;

                    var reason = "";
                    if (packet.DataSize > 1)
                        reason = Encoding.UTF8.GetString(packet.Data, 1, Math.Clamp(packet.DataSize - 1, 1, 128));

                    Error = reason;
                    Debug.Log("connection", $"closed reason='{reason}'");
                    return false;
                }

                if (State == ConnectionState.OFFLINE && 
                    msg == ConnectionMessages.CONNECT)
                {
                    Reset();

                    State = ConnectionState.PENDING;
                    EndPoint = remote;
                    LastSendTime = now;
                    LastReceiveTime = now;
                    ConnectedAt = now;

                    SendControlMsg(ConnectionMessages.CONNECTACCEPT, "");
                    Debug.Log("connection", "got connection, sending connect+accept");
                }
                else if (State == ConnectionState.CONNECT &&
                    msg == ConnectionMessages.CONNECTACCEPT)
                {
                    LastReceiveTime = now;
                    State = ConnectionState.ONLINE;
                    SendControlMsg(ConnectionMessages.ACCEPT, "");
                    Debug.Log("connection", "got connect+accept, sending accept. connection online");
                }
            }
            else if (State == ConnectionState.PENDING)
            {
                State = ConnectionState.ONLINE;
                Debug.Log("connection", "connecting online");
            }

            if (State == ConnectionState.ONLINE)
            {
                LastReceiveTime = now;
                AckChunks(packet.Ack);
            }

            return true;
        }

        public override void SignalResend()
        {
            ResendQueueConstruct.Flags |= PacketFlags.RESEND;
        }

        public override int Flush()
        {
            var numChunks = ResendQueueConstruct.NumChunks;
            if (numChunks == 0 && ResendQueueConstruct.Flags == PacketFlags.NONE)
                return 0;

            ResendQueueConstruct.Ack = Ack;
            NetworkCore.SendPacket(UdpClient, EndPoint, ResendQueueConstruct);
            LastSendTime = Time.Get();
            ResetQueueConstruct();

            return numChunks;
        }

        public override bool QueueChunkEx(ChunkFlags flags, int dataSize, byte[] data, int sequence)
        {
            if (ResendQueueConstruct.DataSize + 
                dataSize + NetworkCore.PACKET_HEADER_SIZE > NetworkCore.MAX_PAYLOAD)
            {
                Flush();
            }
            
            var header = new NetworkChunkHeader
            {
                Flags = flags,
                Size = dataSize,
                Sequence = sequence
            };

            var chunkDataOffset = ResendQueueConstruct.DataSize;
            chunkDataOffset = header.Pack(ResendQueueConstruct.Data, chunkDataOffset);

            Buffer.BlockCopy(data, 0, ResendQueueConstruct.Data, chunkDataOffset, dataSize);
            chunkDataOffset += dataSize;

            ResendQueueConstruct.NumChunks++;
            ResendQueueConstruct.DataSize = chunkDataOffset;

            if (flags.HasFlag(ChunkFlags.VITAL) && !flags.HasFlag(ChunkFlags.RESEND))
            {
                BufferSize += SIZEOF_NETWORK_CHUNK_RESEND + dataSize;
                if (BufferSize >= BUFFERSIZE)
                {
                    Disconnect("too weak connection (out of buffer)");
                    return false;
                }

                var resend = new NetworkChunkResend
                {
                    Sequence = sequence,
                    Flags = flags,
                    DataSize = dataSize,
                    Data = new byte[dataSize],
                    FirstSendTime = Time.Get(),
                    LastSendTime = Time.Get()
                };

                Buffer.BlockCopy(data, 0, resend.Data, 0, dataSize);
                ResendQueue.Enqueue(resend);
            }

            return true;
        }

        public override bool QueueChunk(ChunkFlags flags, byte[] data, int dataSize)
        {
            if (flags.HasFlag(ChunkFlags.VITAL))
                Sequence = (Sequence + 1) % NetworkCore.MAX_SEQUENCE;
            return QueueChunkEx(flags, dataSize, data, Sequence);
        }

        public override void SendControlMsg(ConnectionMessages msg, string extra)
        {
            LastSendTime = Time.Get();
            NetworkCore.SendControlMsg(UdpClient, EndPoint, Ack, msg, extra);
        }

        protected override void ResendChunk(NetworkChunkResend resend)
        {
            QueueChunkEx(resend.Flags | ChunkFlags.RESEND, resend.DataSize,
                resend.Data, resend.Sequence);
            resend.LastSendTime = Time.Get();
        }

        protected override void Resend()
        {
            foreach (var chunkResend in ResendQueue)
            {
                ResendChunk(chunkResend);
            }
        }

        protected override void AckChunks(int ack)
        {
            while (true)
            {
                if (ResendQueue.Count == 0)
                    return;

                if (ResendQueue.TryPeek(out var chunk))
                {
                    if (NetworkCore.IsSeqInBackroom(chunk.Sequence, ack))
                    {
                        ResendQueue.Dequeue();
                        BufferSize -= SIZEOF_NETWORK_CHUNK_RESEND + chunk.DataSize;
                    }
                    else return;
                }
                else return;
            }
        }
    }
}
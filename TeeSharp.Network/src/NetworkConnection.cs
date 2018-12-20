using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Enums;
using TeeSharp.Core;
using TeeSharp.Network.Enums;
using Math = System.Math;

namespace TeeSharp.Network
{
    public class NetworkConnection : BaseNetworkConnection
    {
        public NetworkConnection()
        {
            Error = string.Empty;
            ResendQueueConstruct = new NetworkChunkConstruct();
            ResendQueue = new Queue<NetworkChunkResend>();
        }
        
        public override bool Connect(IPEndPoint endPoint)
        {
            if (State != ConnectionState.OFFLINE)
                return false;

            Reset();
            EndPoint = endPoint;
            State = ConnectionState.CONNECT;
            Token = Secure.RandomUInt32();
            SendConnect();
            return true;
        }

        public override void SendConnect()
        {
            var connect = new byte[512];
            Token.ToByteArray(connect, 4);
            SendControlMsg(ConnectionMessages.CONNECT, connect);
        }

        public override bool Accept(IPEndPoint addr, uint token)
        {
            if (State != ConnectionState.OFFLINE)
                return false;

            Reset();
            EndPoint = addr;
            State = ConnectionState.ONLINE;
            LastReceiveTime = Time.Get();
            Token = token;

            Debug.Log("connection", "connection online");
            return true;
        }

        public override bool AcceptLegacy(IPEndPoint addr)
        {
            if (State != ConnectionState.OFFLINE)
                return false;

            Reset();
            EndPoint = addr;
            State = ConnectionState.ONLINE;
            LastReceiveTime = Time.Get();

            Token = 0;
            UseToken = false;
            UnknownAck = true;
            Sequence = NetworkCore.COMPATIBILITY_SEQ;

            Debug.Log("connection", "legacy connecting online");
            return true;
        }

        public override void ResetQueueConstruct()
        {
            ResendQueueConstruct.DataSize = 0;
            ResendQueueConstruct.Flags = PacketFlags.None;
            ResendQueueConstruct.NumChunks = 0;
            ResendQueueConstruct.Ack = 0;
        }

        public override void Reset()
        {
            Sequence = 0;
            UnknownAck = false;
            Ack = 0;
            PeerAck = 0;
            RemoteClosed = false;

            State = ConnectionState.OFFLINE;
            ConnectedAt = 0;
            LastReceiveTime = 0;
            LastSendTime = 0;
            UseToken = true;
            Token = 0;

            EndPoint = null;

            ResendQueue.Clear();
            BufferSize = 0;
            
            Error = string.Empty;
            ResetQueueConstruct();
        }

        public override void Init(UdpClient udpClient)
        {
            Reset();

            Config = Kernel.Get<BaseConfig>();
            Error = string.Empty;
            UdpClient = udpClient;
        }

        public override void Update()
        {
            if (State == ConnectionState.OFFLINE ||
                State == ConnectionState.ERROR)
            {
                return;
            }

            if (State != ConnectionState.OFFLINE &&
                State != ConnectionState.CONNECT &&
                (Time.Get() - LastReceiveTime) > Time.Freq() * Config["ConnTimeout"])
            {
                State = ConnectionState.ERROR;
                Error = "Timeout";
                return;
            }

            if (ResendQueue.Count > 0)
            {
                var resend = ResendQueue.Peek();
                if (Time.Get() - resend.FirstSendTime > Time.Freq() * Config["ConnTimeout"])
                {
                    State = ConnectionState.ERROR;
                    Error = $"Too weak connection (not acked for {Config["ConnTimeout"]} seconds)";
                }
                else if (Time.Get() - resend.LastSendTime > Time.Freq())
                {
                    ResendChunk(resend);
                }
            }

            if (State == ConnectionState.ONLINE)
            {
                if (Time.Get() - LastSendTime > Time.Freq() / 2)
                {
                    var flushedChunks = Flush();
                    if (flushedChunks != 0)
                        Debug.Log("connection", $"flushed connection due to timeout. {flushedChunks} chunks.");
                }

                if (Time.Get() - LastSendTime > Time.Freq())
                    SendControlMsg(ConnectionMessages.KEEPALIVE, "");
            }
            else if (State == ConnectionState.CONNECT)
            {
                if (Time.Get() - LastSendTime > Time.Freq() / 2)
                    SendControlMsg(ConnectionMessages.CONNECT, "");
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
            if (packet.Flags.HasFlag(PacketFlags.RESEND))
                Resend();

            if (UseToken)
            {
                if (!packet.Flags.HasFlag(PacketFlags.TOKEN))
                {
                    if (!packet.Flags.HasFlag(PacketFlags.CONTROL) || packet.DataSize < 1)
                    {
                        Debug.Log("connection", "dropping msg without token");
                        return false;
                    }

                    if (packet.Data[0] == (int) ConnectionMessages.CONNECTACCEPT)
                    {
                        if (!Config["ClAllowOldServers"])
                        {
                            Debug.Log("connection", "dropping connect+accept without token");
                            return false;
                        }
                    }
                    else
                    {
                        Debug.Log("connection", "dropping ctrl msg without token");
                        return false;
                    }
                }
                else
                {
                    if (packet.Token != Token)
                    {
                        Debug.Log("connection", $"dropping msg with invalid token, wanted={Token} got={packet.Token}");
                        return false;
                    }
                }
            }

            if (Sequence >= PeerAck)
            {
                if (packet.Ack < PeerAck || packet.Ack > Sequence)
                    return false;
            }
            else
            {
                if (packet.Ack < PeerAck && packet.Ack > Sequence)
                    return false;
            }

            PeerAck = packet.Ack;
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
                else
                {
                    if (State == ConnectionState.CONNECT)
                    {
                        if (msg == ConnectionMessages.CONNECTACCEPT)
                        {
                            if (packet.Flags.HasFlag(PacketFlags.TOKEN))
                            {
                                if (packet.DataSize < 1 + 4)
                                {
                                    Debug.Log("connection", $"got short connect+accept, size={packet.DataSize}");
                                    return true;
                                }

                                Token = packet.Data.ToUInt32(1);
                            }
                            else
                            {
                                UseToken = false;
                            }

                            LastReceiveTime = Time.Get();
                            State = ConnectionState.ONLINE;
                            Debug.Log("connection", "got connect+accept, sending accept. connection online");
                        }
                    }
                }
            }

            if (State == ConnectionState.ONLINE)
            {
                LastReceiveTime = Time.Get();
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
            if (numChunks == 0 && ResendQueueConstruct.Flags == PacketFlags.None)
                return 0;

            ResendQueueConstruct.Ack = Ack;

            if (UseToken)
            {
                ResendQueueConstruct.Flags |= PacketFlags.TOKEN;
                ResendQueueConstruct.Token = Token;
            }

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

        public override void SendControlMsg(ConnectionMessages msg, byte[] extra)
        {
            LastSendTime = Time.Get();
            var useToken = UseToken && msg != ConnectionMessages.CONNECT;
            NetworkCore.SendControlMsg(UdpClient, EndPoint, Ack, useToken,
                Token, msg, extra);
        }

        public override void SendControlMsg(ConnectionMessages msg, string extra)
        {
            LastSendTime = Time.Get();
            var useToken = UseToken && msg != ConnectionMessages.CONNECT;
            NetworkCore.SendControlMsg(UdpClient, EndPoint, Ack, useToken, 
                Token, msg, extra);
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
            do
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
            } while (true);
        }
    }
}
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
        protected override void Reset()
        {
            Sequence = 0;
            Ack = 0;
            PeerAck = 0;
            RemoteClosed = false;

            State = ConnectionState.Offline;
            LastSendTime = 0;
            LastReceiveTime = 0;
            ConnectedAt = 0;
            Token = TokenHelper.TokenNone;
            PeerToken = TokenHelper.TokenNone;
            EndPoint = null;

            ChunksForResends.Clear();
            SizeOfChunksForResends = 0;
            Error = string.Empty;
            ResetChunkConstruct();
        }

        protected virtual void ResetChunkConstruct()
        {
            ResendChunkConstruct.DataSize = 0;
            ResendChunkConstruct.Flags = PacketFlags.None;
            ResendChunkConstruct.ResponseToken = 0;
            ResendChunkConstruct.Token = 0;
            ResendChunkConstruct.Ack = 0;
            ResendChunkConstruct.NumChunks = 0;
        }

        public override void SetToken(uint token)
        {
            if (State != ConnectionState.Offline)
                return;

            Token = token;
        }

        public override void Init(UdpClient udpClient)
        {
            Reset();
            UdpClient = udpClient;
        }

        protected override void AckChunks(int ack)
        {
            while (true)
            {
                if (ChunksForResends.Count == 0)
                    return;

                if (NetworkHelper.IsSequenceInBackroom(ChunksForResends[0].Sequence, ack))
                {
                    SizeOfChunksForResends -= 32 + ChunksForResends[0].DataSize;
                    ChunksForResends.RemoveAt(0); // TODO make Unidirectional list
                }
                else 
                    return;
            }
        }

        protected override void SignalResend()
        {
            ResendChunkConstruct.Flags |= PacketFlags.Resend;
        }

        public override int Flush()
        {
            var numChunks = ResendChunkConstruct.NumChunks;
            if (numChunks == 0 && ResendChunkConstruct.Flags == PacketFlags.None)
                return 0;

            ResendChunkConstruct.Ack = Ack;
            ResendChunkConstruct.Token = PeerToken;

            NetworkHelper.SendPacket(UdpClient, EndPoint, ResendChunkConstruct);
            ResetChunkConstruct();

            return numChunks;
        }

        protected override bool QueueChunkEx(ChunkFlags flags, byte[] data, int dataSize, 
            int sequence)
        {
            if (ResendChunkConstruct.DataSize + dataSize + NetworkHelper.MaxChunkHeaderSize > 
                ResendChunkConstruct.Data.Length || 
                ResendChunkConstruct.NumChunks >= NetworkHelper.MaxPacketChunks)
            {
                Flush();
            }

            var header = new ChunkHeader
            {
                Flags = flags,
                Size = dataSize,
                Sequence = sequence
            };

            var dataOffset = ResendChunkConstruct.DataSize;
            dataOffset = header.Pack(ResendChunkConstruct.Data, dataOffset);

            Buffer.BlockCopy(data, 0, ResendChunkConstruct.Data, dataOffset, dataSize);
            ResendChunkConstruct.NumChunks++;
            ResendChunkConstruct.DataSize = dataOffset + dataSize;

            if (flags.HasFlag(ChunkFlags.Vital) && !flags.HasFlag(ChunkFlags.Resend))
            {
                SizeOfChunksForResends += 32 + dataSize;

                if (SizeOfChunksForResends >= NetworkHelper.ConnectionBufferSize)
                {
                    Disconnect("too weak connection (out of buffer)");
                    return false;
                }

                var resend = new ChunkResend
                {
                    Sequence = sequence,
                    Flags = flags,
                    DataSize = dataSize,
                    Data = new byte[dataSize],
                    FirstSendTime = Time.Get(),
                    LastSendTime = Time.Get()
                };

                Buffer.BlockCopy(data, 0, resend.Data, 0, dataSize);
                ChunksForResends.Add(resend);
            }

            return true;
        }

        public override bool QueueChunk(ChunkFlags flags, byte[] data, int dataSize)
        {
            if (flags.HasFlag(ChunkFlags.Vital))
                Sequence = (Sequence + 1) % NetworkHelper.MaxSequence;
            return QueueChunkEx(flags, data, dataSize, Sequence);
        }

        protected override void SendConnectionMsg(ConnectionMessages msg, 
            byte[] extra, int extraSize)
        {
            LastSendTime = Time.Get();
            NetworkHelper.SendConnectionMsg(UdpClient, EndPoint, 
                PeerToken, Ack, msg, extra, extraSize);
        }

        protected override void SendConnectionMsg(ConnectionMessages msg, string extra)
        {
            LastSendTime = Time.Get();
            NetworkHelper.SendConnectionMsg(UdpClient, EndPoint,
                PeerToken, Ack, msg, extra);
        }

        protected override void SendConnectionMsgWithToken(ConnectionMessages msg)
        {
            LastSendTime = Time.Get();
            NetworkHelper.SendConnectionMsgWithToken(UdpClient, EndPoint, PeerToken, 0,
                msg, Token, true);
        }

        public override void SendPacketConnless(byte[] data, int dataSize)
        {
            NetworkHelper.SendPacketConnless(UdpClient, EndPoint, PeerToken, Token, data, dataSize);
        }

        protected override void ResendChunk(ChunkResend resend)
        {
            QueueChunkEx(resend.Flags | ChunkFlags.Resend, resend.Data,
                resend.DataSize, resend.Sequence);
            resend.LastSendTime = Time.Get();
        }

        protected override void Resend()
        {
            for (var i = 0; i < ChunksForResends.Count; i++)
            {
                ResendChunk(ChunksForResends[i]);
            }
        }

        public override bool Connect(IPEndPoint endPoint)
        {
            if (State != ConnectionState.Offline)
                return false;

            Reset();
            EndPoint = endPoint;
            PeerToken = TokenHelper.TokenNone;
            SetToken(GenerateToken(endPoint));
            State = ConnectionState.Token;
            SendConnectionMsgWithToken(ConnectionMessages.Token);
            return true;
        }

        public override void Disconnect(string reason)
        {
            if (State == ConnectionState.Offline)
                return;

            if (!RemoteClosed)
            {
                SendConnectionMsg(ConnectionMessages.Close, reason);
                Error = reason;
            }

            Reset();
        }

        public override bool Feed(ChunkConstruct packet, IPEndPoint endPoint)
        {
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

            if (packet.Token == TokenHelper.TokenNone || packet.Token != Token)
                return false;

            if (packet.Flags.HasFlag(PacketFlags.Resend))
                Resend();

            if (packet.Flags.HasFlag(PacketFlags.Connless))
                return true;

            var now = Time.Get();
            if (packet.Flags.HasFlag(PacketFlags.Control))
            {
                var msg = (ConnectionMessages) packet.Data[0];
                if (msg == ConnectionMessages.Close)
                {
                    State = ConnectionState.Error;
                    RemoteClosed = true;

                    string reason = null;
                    if (packet.DataSize > 1)
                    {
                        reason = Encoding.UTF8.GetString(packet.Data, 1, Math.Clamp(packet.DataSize - 1, 1, 128));
                        reason = reason.SanitizeStrong();
                    }

                    Error = reason;
                    Debug.Log("connection", $"closed reason='{reason}'");
                }
                else if (msg == ConnectionMessages.Token)
                {
                    PeerToken = packet.ResponseToken;
                    if (State == ConnectionState.Token)
                    {
                        LastReceiveTime = now;
                        State = ConnectionState.Connect;
                        SendConnectionMsgWithToken(ConnectionMessages.Connect);
                        Debug.Log("connection", $"got token, replying, token={PeerToken:X} mytoken={Token:X}");
                    }
                    else 
                        Debug.Log("connection", $"got token, token={PeerToken:X}");
                }
                else
                {
                    if (State == ConnectionState.Offline)
                    {
                        if (msg == ConnectionMessages.Connect)
                        {
                            Reset();
                            State = ConnectionState.Pending;
                            EndPoint = endPoint;
                            PeerToken = packet.ResponseToken;
                            LastSendTime = now;
                            LastReceiveTime = now;
                            ConnectedAt = now;
                            SendConnectionMsg(ConnectionMessages.ConnectAccept, null);
                            Debug.Log("connection", "got connection, sending connect+accept");
                        }
                    }
                    else if (State == ConnectionState.Connect)
                    {
                        if (msg == ConnectionMessages.ConnectAccept)
                        {
                            LastReceiveTime = now;
                            SendConnectionMsg(ConnectionMessages.Accept, null);
                            State = ConnectionState.Online;
                            Debug.Log("connection", "got connect+accept, sending accept. connection online");
                        }
                    }
                }
            }
            else if (State == ConnectionState.Pending)
            {
                LastReceiveTime = now;
                State = ConnectionState.Online;
                Debug.Log("connection", "connecting online");
            }

            if (State == ConnectionState.Online)
            {
                LastReceiveTime = now;
                AckChunks(packet.Ack);
            }

            return true;
        }

        public override void Update()
        {
            if (State == ConnectionState.Offline || State == ConnectionState.Error)
                return;

            var now = Time.Get();

            if (State != ConnectionState.Offline &&
                State != ConnectionState.Token &&
                now - LastReceiveTime > Time.Freq() * 10)
            {
                State = ConnectionState.Error;
                Error = "Timeout";
            }

            if (ChunksForResends.Count > 0)
            {
                if (now - ChunksForResends[0].FirstSendTime > Time.Freq() * 10)
                {
                    State = ConnectionState.Error;
                    Error = "Too weak connection (not acked for 10 seconds)";
                }
                else if (now - ChunksForResends[0].LastSendTime > Time.Freq())
                {
                    ResendChunk(ChunksForResends[0]);
                }
            }

            if (State == ConnectionState.Online)
            {
                if (now - LastSendTime > Time.Freq() / 2)
                {
                    var flushed = Flush();
                    if (flushed > 0)
                        Debug.Log("connection", $"flushed connection due to timeout. {flushed} chunks.");
                }

                if (now - LastSendTime > Time.Freq())
                    SendConnectionMsg(ConnectionMessages.KeepAlive, null);
            }
            else if (State == ConnectionState.Token)
            {
                if (now - LastSendTime > Time.Freq() / 2)
                    SendConnectionMsgWithToken(ConnectionMessages.Token);
            }
            else if (State == ConnectionState.Connect)
            {
                if (now - LastSendTime > Time.Freq() / 2)
                    SendConnectionMsgWithToken(ConnectionMessages.Connect);
            }
            else if (State == ConnectionState.Pending)
            {
                if (now - LastSendTime > Time.Freq() / 2)
                    SendConnectionMsg(ConnectionMessages.ConnectAccept, null);
            }
        }
    }
}
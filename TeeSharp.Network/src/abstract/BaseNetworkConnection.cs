using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TeeSharp.Common.Config;
using TeeSharp.Core;
using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public abstract class BaseNetworkConnection : BaseInterface
    {
        public const int BUFFERSIZE = 1024 * 32;

        public abstract BaseConfig Config { get; protected set; }
        public abstract ConnectionState State { get; protected set; }
        public abstract long ConnectedAt { get; protected set; }
        public abstract IPEndPoint EndPoint { get; protected set; }
        public abstract string Error { get; protected set; }

        public abstract int Sequence { get; set; }
        public abstract bool UnknownAck { get; set; }
        public abstract int Ack { get; set; }
        public abstract int PeerAck { get; set; }

        public abstract bool UseToken { get; set; }
        public abstract uint Token { get; set; }

        public abstract long LastReceiveTime { get; protected set; }
        public abstract long LastSendTime { get; protected set; }

        protected abstract UdpClient UdpClient { get; set; } 
        protected abstract int BufferSize { get; set; }
        protected abstract bool RemoteClosed { get; set; }
        protected abstract Queue<NetworkChunkResend> ResendQueue { get; set; }
        protected abstract NetworkChunkConstruct ResendQueueConstruct { get; set; }

        public abstract void Disconnect(string reason);
        public abstract bool Connect(IPEndPoint endPoint);
        public abstract void SendConnect();
        public abstract bool Accept(IPEndPoint addr, uint token);
        public abstract bool AcceptLegacy(IPEndPoint addr);

        public abstract void ResetQueueConstruct();
        public abstract void Reset();
        public abstract void Init(UdpClient udpClient);
        public abstract void Update();
        public abstract bool Feed(NetworkChunkConstruct packet, IPEndPoint remote);
        public abstract void SignalResend();
        public abstract int Flush();
        public abstract bool QueueChunkEx(ChunkFlags flags, int dataSize, byte[] data, int sequence);
        public abstract bool QueueChunk(ChunkFlags flags, byte[] data, int dataSize);
        public abstract void SendControlMsg(ConnectionMessages msg, string extra);
        public abstract void SendControlMsg(ConnectionMessages msg, byte[] extra);

        protected abstract void ResendChunk(NetworkChunkResend resend);
        protected abstract void Resend();
        protected abstract void AckChunks(int ack);
    }
}
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TeeSharp.Core;
using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public struct NetworkConnectionConfig
    {
        public int ConnectionTimeout;
    }

    public abstract class BaseNetworkConnection : BaseInterface
    {
        public const int BUFFERSIZE = 1024 * 32;

        public abstract NetworkConnectionConfig Config { get; set; }
        public abstract ConnectionState State { get; protected set; }
        public abstract long ConnectedAt { get; protected set; }
        public abstract IPEndPoint EndPoint { get; protected set; }
        public abstract string Error { get; protected set; }

        public abstract int Sequence { get; set; }
        public abstract int Ack { get; set; }

        public abstract long LastReceiveTime { get; protected set; }
        public abstract long LastSendTime { get; protected set; }

        protected abstract UdpClient UdpClient { get; set; } 
        protected abstract int BufferSize { get; set; }
        protected abstract bool RemoteClosed { get; set; }
        protected abstract Queue<NetworkChunkResend> ResendQueue { get; set; }
        protected abstract NetworkChunkConstruct ResendQueueConstruct { get; set; }

        public abstract void Disconnect(string reason);
        public abstract bool Connect(IPEndPoint endPoint);

        public abstract void ResetQueueConstruct();
        public abstract void Reset();
        public abstract void Init(UdpClient udpClient, NetworkConnectionConfig config);
        public abstract void Update();
        public abstract bool Feed(NetworkChunkConstruct packet, IPEndPoint remote);
        public abstract void SignalResend();
        public abstract int Flush();
        public abstract bool QueueChunkEx(ChunkFlags flags, int dataSize, byte[] data, int sequence);
        public abstract bool QueueChunk(ChunkFlags flags, byte[] data, int dataSize);
        public abstract void SendControlMsg(ConnectionMessages msg, string extra);

        protected abstract void ResendChunk(NetworkChunkResend resend);
        protected abstract void Resend();
        protected abstract void AckChunks(int ack);
    }
}
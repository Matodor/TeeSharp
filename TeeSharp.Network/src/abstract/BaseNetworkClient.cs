using System.Net;
using System.Net.Sockets;
using TeeSharp.Core;
using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public struct NetworkClientConfig
    {
        public IPEndPoint LocalEndPoint;
        public int ConnectionTimeout;
    }

    public abstract class BaseNetworkClient : BaseInterface
    {
        public abstract BaseNetworkConnection Connection { get; protected set; }
        public abstract UdpClient UdpClient { get; protected set; }

        protected abstract BaseChunkReceiver ChunkReceiver { get; set; }

        public abstract void Init();
        public abstract bool Open(NetworkClientConfig config);
        public abstract void Close();

        public abstract void Disconnect(string reason);
        public abstract bool Connect(IPEndPoint endPoint);
        public abstract void Update();

        public abstract bool Receive(out NetworkChunk packet);
        public abstract void Send(NetworkChunk packet);

        public abstract void Flush();
        public abstract bool GotProblems();
    }
}
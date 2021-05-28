using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using TeeSharp.Core.MinIoC;

namespace TeeSharp.Network
{
    public abstract class BaseNetworkServer : IContainerService
    {
        public Container Container { get; set; }
        public EndPoint BindAddress => Socket?.Client.LocalEndPoint;
        
        protected BaseChunkFactory ChunkFactory { get; set; }
        protected UdpClient Socket { get; set; }
        
        public abstract void Init();
        public abstract void Update();
        // ReSharper disable once InconsistentNaming
        public abstract bool Open(IPEndPoint localEP);
        public abstract bool Receive(out NetworkMessage netMsg, ref SecurityToken responseToken);
        public abstract SecurityToken GetToken(IPEndPoint endPoint);
    }
}
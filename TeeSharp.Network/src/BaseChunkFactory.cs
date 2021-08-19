using System.Net;
using TeeSharp.Core.MinIoC;

namespace TeeSharp.Network
{
    public abstract class BaseChunkFactory : IContainerService
    {
        public Container.IScope Container { get; set; }
        public NetworkPacket NetworkPacket { get; protected set; }
        
        protected bool HasError { get; set; }
        protected IPEndPoint EndPoint { get; set; }
        protected int ClientId { get; set; }
        protected BaseNetworkConnection Connection { get; set; }
        protected int ProcessedChunks { get; set; }

        public abstract void Init();
        public abstract void Reset();
        public abstract void Start(IPEndPoint endPoint, BaseNetworkConnection connection, int clientId);
        public abstract bool TryGetMessage(out NetworkMessage netMsg);
    }
}
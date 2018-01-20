using System.Net;
using TeeSharp.Core;
using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public abstract class BaseNetworkConnection : BaseInterface
    {
        public abstract ConnectionState State { get; protected set; }
        public abstract long ConnectedAt { get; protected set; }
        public abstract IPEndPoint EndPoint { get; protected set; }
        public abstract string Error { get; protected set; }

        protected abstract BaseNetworkServer NetworkServer { get; set; } 
        protected abstract long LastReceiveTime { get; set; }

        public abstract void Init(BaseNetworkServer networkServer, bool closeMsg);
        public abstract void Update();
        public abstract void Disconnect(string reason);
        public abstract bool Feed(NetworkChunkConstruct packet, IPEndPoint remote);
        public abstract void Flush();
        public abstract bool QueueChunk(SendFlags flags, byte[] data, int dataSize);
    }
}
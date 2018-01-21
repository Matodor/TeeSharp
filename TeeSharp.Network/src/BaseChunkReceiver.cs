using System.Net;
using TeeSharp.Core;

namespace TeeSharp.Network
{
    public abstract class BaseChunkReceiver : BaseInterface
    {
        public abstract NetworkChunkConstruct ChunkConstruct { get; set; }

        protected abstract bool Valid { get; set; }
        protected abstract int CurrentChunk { get; set; }
        protected abstract int ClientId { get; set; }
        protected abstract BaseNetworkConnection Connection { get; set; }
        protected abstract IPEndPoint EndPoint { get; set; }

        public abstract void Clear();
        public abstract bool FetchChunk(out NetworkChunk packet);
        public abstract void Start(IPEndPoint remote, BaseNetworkConnection connection,
            int clientId);
    }
}
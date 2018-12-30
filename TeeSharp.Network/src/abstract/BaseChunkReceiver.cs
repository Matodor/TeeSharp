using System.Net;
using TeeSharp.Core;

namespace TeeSharp.Network
{
    public abstract class BaseChunkReceiver : BaseInterface
    {
        public virtual ChunkConstruct ChunkConstruct { get; protected set; }

        protected virtual bool Valid { get; set; }
        protected virtual int CurrentChunk { get; set; }
        protected virtual int ClientId { get; set; }
        protected virtual BaseNetworkConnection Connection { get; set; }
        protected virtual IPEndPoint EndPoint { get; set; }

        public abstract void Clear();
        public abstract void Start(IPEndPoint remote, 
            BaseNetworkConnection connection, int clientId);
        public abstract bool FetchChunk(ref Chunk packet);
    }
}
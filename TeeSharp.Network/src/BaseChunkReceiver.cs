using System.Net;
using TeeSharp.Core;

namespace TeeSharp.Network
{
    public abstract class BaseChunkReceiver : BaseInterface
    {
        public abstract NetworkChunkConstruct ChunkConstruct { get; set; }

        public abstract bool FetchChunk(out NetworkChunk packet);
        public abstract void Start(IPEndPoint remote, int connectionId);
    }
}
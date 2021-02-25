using System.Net;

namespace TeeSharp.Network
{
    public class ChunkFactory : BaseChunkFactory
    {
        public override void Init()
        {
            Chunks = new NetworkChunks
            {
                Data = new byte[NetworkConstants.MaxPayload], 
                ExtraData = new byte[4],
            };
        }

        public override void Reset()
        {
            HasError = true;
            EndPoint = null;
            ClientId = -1;
            Connection = null;
            ProcessedChunks = 0;
        }

        public override void Start(IPEndPoint endPoint, BaseNetworkConnection connection, int clientId)
        {
            HasError = false;
            EndPoint = endPoint;
            ClientId = clientId;
            Connection = connection;
            ProcessedChunks = 0;
        }

        public override bool TryGet(out NetworkMessage netMsg)
        {
            if (HasError)
            {
                netMsg = null;
                return false;
            }
            
            netMsg = null;
            return false;
        }
    }
}
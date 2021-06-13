using System.Net;

namespace TeeSharp.Network
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class ChunkFactory : BaseChunkFactory
    {
        public override void Init()
        {
            NetworkPacket = new NetworkPacket
            {
                Data = new byte[NetworkConstants.MaxPayload], 
                ExtraData = new byte[NetworkConstants.PacketExtraDataSize],
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

        public override bool TryGetMessage(out NetworkMessage netMsg)
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
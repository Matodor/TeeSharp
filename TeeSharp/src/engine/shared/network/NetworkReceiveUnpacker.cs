using System.Net;

namespace TeeSharp
{
    public class NetworkReceiveUnpacker
    {
        public readonly NetPacketConstruct PacketConstruct;

        public NetworkReceiveUnpacker()
        {
            PacketConstruct = new NetPacketConstruct();
        }

        public void Clear()
        {
            
        }

        public void Start(IPEndPoint addr, NetworkConnection connection, int clientId)
        {
            
        }

        public bool FetchChunk(out NetChunk packet)
        {
            packet = new NetChunk();
            return false;
        }
    }
}
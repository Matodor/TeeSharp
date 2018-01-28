using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public class NetworkChunkConstruct
    {
        public PacketFlags Flags;
        public int Ack;
        public int NumChunks;
        public int DataSize;
        public readonly byte[] Data = new byte[NetworkCore.MAX_PAYLOAD];
    }
}
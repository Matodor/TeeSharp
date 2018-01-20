using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public class NetworkChunkConstruct
    {
        public PacketFlags Flags;
        public int Ack;
        public int NumChunks;
        public int DataSize;
        public byte[] Data;
    }
}
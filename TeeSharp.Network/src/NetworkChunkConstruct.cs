using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public class NetworkChunkConstruct
    {
        public PacketFlags Flags;
        public int Ack;
        public int NumChunks;
        public int DataSize;
        public readonly byte[] Data;
        public uint Token;

        public NetworkChunkConstruct()
        {
            Data = new byte[NetworkCore.MAX_PAYLOAD];
        }

        public NetworkChunkConstruct(int dataSize)
        {
            Data = new byte[dataSize];
        }
    }
}
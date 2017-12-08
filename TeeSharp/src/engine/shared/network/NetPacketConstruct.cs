namespace TeeSharp
{
    public class NetPacketConstruct
    {
        public PacketFlag Flags;
        public int Ack;
        public int NumChunks;
        public int DataSize;
        public byte[] ChunkData;
    }
}
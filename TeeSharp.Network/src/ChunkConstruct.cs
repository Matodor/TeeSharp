using TeeSharp.Network.Enums;
using Token = System.UInt32;

namespace TeeSharp.Network
{
    public class ChunkConstruct
    {
        public Token Token { get; set; }
        public Token ResponseToken { get; set; }

        public PacketFlags Flags { get; set; }
        public int Ack { get; set; }
        public int NumChunks { get; set; }
        public int DataSize { get; set; }
        public byte[] Data { get; }

        public ChunkConstruct(int bufferSize)
        {
            Data = new byte[bufferSize];
        }
    }
}
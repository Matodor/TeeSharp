using TeeSharp.Network.Enums;
using Token = System.UInt32;

namespace TeeSharp.Network
{
    public class NetworkChunkConstruct
    {
        public Token Token { get; set; }
        public Token ResponseToken { get; set; }

        public PacketFlags Flags { get; set; }
        public int Ack { get; set; }
        public int NumChunks { get; set; }
        public int DataSize { get; set; }
        public byte[] Buffer { get; }

        public NetworkChunkConstruct(int bufferSize)
        {
            Buffer = new byte[bufferSize];
        }
    }
}
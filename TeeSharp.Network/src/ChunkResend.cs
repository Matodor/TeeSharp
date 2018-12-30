using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public class ChunkResend
    {
        public int Sequence { get; set; }
        public ChunkFlags Flags { get; set; }
        public int DataSize { get; set; }
        public byte[] Data { get; set; }
        public long FirstSendTime { get; set; }
        public long LastSendTime { get; set; }
    }
}
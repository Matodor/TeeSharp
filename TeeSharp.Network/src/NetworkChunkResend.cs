using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public class NetworkChunkResend
    {
        public int Sequence;
        public ChunkFlags Flags;
        public int DataSize;
        public byte[] Data;
        public long FirstSendTime;
        public long LastSendTime;
    }
}
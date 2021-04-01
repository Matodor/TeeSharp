namespace TeeSharp.Network
{
    public class ChunksData
    {
        public ChunkFlags Flags { get; set; } = ChunkFlags.None;
        public int Ack { get; set; }
        public int Count { get; set; }
        public int DataSize { get; set; }
        public byte[] Data { get; set; }
        public byte[] ExtraData { get; set; }
    }
}
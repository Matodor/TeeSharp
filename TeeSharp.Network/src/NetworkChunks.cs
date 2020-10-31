namespace TeeSharp.Network
{
    public class NetworkChunks
    {
        public PacketFlags Flags { get; set; }
        public int Ack { get; set; }
        public int ChunksCount { get; set; }
        public int DataSize { get; set; }
        public readonly byte[] Data = new byte[NetworkConstants.MaxPayload];
        public readonly byte[] ExtraData = new byte[4];
    }
}
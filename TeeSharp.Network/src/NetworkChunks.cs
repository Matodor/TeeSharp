namespace TeeSharp.Network
{
    public class NetworkChunks
    {
        public PacketFlags Flags { get; set; }
        public int Ack { get; set; }
        public int ChunksCount { get; set; }
        public int DataSize { get; set; }
        public byte[] Data { get; } = new byte[NetworkConstants.MaxPayload];
    }
}
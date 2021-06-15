namespace TeeSharp.Network
{
    public class NetworkPacket
    {
        public PacketFlags Flags { get; set; } = PacketFlags.None;
        
        public int Ack { get; set; }
        
        public int ChunksCount { get; set; }
        
        public int DataSize { get; set; }
        
        public byte[] Data { get; set; }
        
        /// <summary>
        /// Used only for master server info extended
        /// </summary>
        public byte[] ExtraData { get; set; }
    }
}
namespace TeeSharp.Network
{
    public static class NetworkConstants
    {
        public static readonly byte[] IpV4Mapping = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255};
        public static readonly byte[] PacketHeaderExtended = {(byte) 'x', (byte) 'e'};
        
        public static readonly SecurityToken SecurityTokenUnknown = -1;
        public static readonly SecurityToken SecurityTokenUnsupported = 0;
        
        public const int MaxPayload = MaxPacketSize - PacketConnLessDataOffset;
        public const int MaxPacketSize = 1400;
        public const int PacketHeaderSize = 3;
        public const int PacketConnLessDataOffset = 6;
    }
}
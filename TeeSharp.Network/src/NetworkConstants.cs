namespace TeeSharp.Network;

public static class NetworkConstants
{
    public static readonly byte[] IpV4Mapping = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255};
    public static readonly byte[] PacketHeaderExtended = {(byte) 'x', (byte) 'e'};

    public const int MaxPayload = MaxPacketSize - PacketConnectionLessDataOffset;
    public const int MaxPacketSize = 1400;
    public const int PacketExtraDataSize = 4;
    public const int PacketHeaderSize = 3;
    public const int PacketConnectionLessDataOffset = 6;
}

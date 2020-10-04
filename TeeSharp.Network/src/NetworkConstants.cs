namespace TeeSharp.Network
{
    public static class NetworkConstants
    {
        public static readonly byte[] IpV4Mapping = new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255};
        
        public const int MaxPacketSize = 1400;
    }
}
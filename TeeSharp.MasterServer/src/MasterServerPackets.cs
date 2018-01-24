namespace TeeSharp.MasterServer
{
    public static class MasterServerPackets
    {
        public static readonly byte[] SERVERBROWSE_HEARTBEAT = Packet('b', 'e', 'a', '2');

        public static readonly byte[] SERVERBROWSE_GETLIST =   Packet('r', 'e', 'q', '2');
        public static readonly byte[] SERVERBROWSE_LIST =      Packet('l', 'i', 's', '2');

        public static readonly byte[] SERVERBROWSE_GETCOUNT =  Packet('c', 'o', 'u', '2');
        public static readonly byte[] SERVERBROWSE_COUNT =     Packet('s', 'i', 'z', '2');

        public static readonly byte[] SERVERBROWSE_GETINFO =   Packet('g', 'i', 'e', '3');
        public static readonly byte[] SERVERBROWSE_INFO =      Packet('i', 'n', 'f', '3');

        public static readonly byte[] SERVERBROWSE_GETINFO_64_LEGACY = Packet('f', 's', 't', 'd');
        public static readonly byte[] SERVERBROWSE_INFO_64_LEGACY =    Packet('d', 't', 's', 'f');

        //public static readonly byte[] SERVERBROWSE_INFO_EXTENDED =      Packet('i', 'e', 'x', 't');
        //public static readonly byte[] SERVERBROWSE_INFO_EXTENDED_MORE = Packet('i', 'e', 'x', '+');

        public static readonly byte[] SERVERBROWSE_FWCHECK =    Packet('f', 'w', '?', '?');
        public static readonly byte[] SERVERBROWSE_FWRESPONSE = Packet('f', 'w', '!', '!');
        public static readonly byte[] SERVERBROWSE_FWOK =       Packet('f', 'w', 'o', 'k');
        public static readonly byte[] SERVERBROWSE_FWERROR =    Packet('f', 'w', 'e', 'r');

        private static byte[] Packet(char b1, char b2, char b3, char b4)
        {
            return new byte[] {255, 255, 255, 255, (byte) b1, (byte) b2, (byte) b3, (byte) b4};
        }
    }
}

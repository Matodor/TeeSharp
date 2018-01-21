namespace TeeSharp.MasterServer
{
    public static class MasterServerPackets
    {
        public static readonly byte[] SERVERBROWSE_HEARTBEAT = { 255, 255, 255, 255, 98, 101, 97, 50 };

        public static readonly byte[] SERVERBROWSE_GETLIST = { 255, 255, 255, 255, 114, 101, 113, 50 };
        public static readonly byte[] SERVERBROWSE_LIST = { 255, 255, 255, 255, 108, 105, 115, 50 };

        public static readonly byte[] SERVERBROWSE_GETCOUNT = { 255, 255, 255, 255, 99, 111, 117, 50 };
        public static readonly byte[] SERVERBROWSE_COUNT = { 255, 255, 255, 255, 115, 105, 122, 50 };

        public static readonly byte[] SERVERBROWSE_GETINFO = { 255, 255, 255, 255, 103, 105, 101, 51 };
        public static readonly byte[] SERVERBROWSE_INFO = { 255, 255, 255, 255, 105, 110, 102, 51 };

        public static readonly byte[] SERVERBROWSE_GETINFO64 = { 255, 255, 255, 255, 102, 115, 116, 100 };
        public static readonly byte[] SERVERBROWSE_INFO64 = { 255, 255, 255, 255, 100, 116, 115, 102 };

        public static readonly byte[] SERVERBROWSE_FWCHECK = { 255, 255, 255, 255, 102, 119, 63, 63 };
        public static readonly byte[] SERVERBROWSE_FWRESPONSE = { 255, 255, 255, 255, 102, 119, 33, 33 };
        public static readonly byte[] SERVERBROWSE_FWOK = { 255, 255, 255, 255, 102, 119, 111, 107 };
        public static readonly byte[] SERVERBROWSE_FWERROR = { 255, 255, 255, 255, 102, 119, 101, 114 };

        public static readonly byte[] SERVERBROWSE_HEARTBEAT_LEGACY = { 255, 255, 255, 255, 98, 101, 97, 116 };

        public static readonly byte[] SERVERBROWSE_GETLIST_LEGACY = { 255, 255, 255, 255, 114, 101, 113, 116 };
        public static readonly byte[] SERVERBROWSE_LIST_LEGACY = { 255, 255, 255, 255, 108, 105, 115, 116 };

        public static readonly byte[] SERVERBROWSE_GETCOUNT_LEGACY = { 255, 255, 255, 255, 99, 111, 117, 110 };
        public static readonly byte[] SERVERBROWSE_COUNT_LEGACY = { 255, 255, 255, 255, 115, 105, 122, 101 };
    }
}

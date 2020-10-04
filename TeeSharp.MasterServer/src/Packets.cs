using System;

namespace TeeSharp.MasterServer
{
    public static class Packets
    {
        public static readonly byte[] Heartbeat = Packet("bea2");
        public static readonly byte[] GetList = Packet("req2");
        public static readonly byte[] List = Packet("lis2");
        public static readonly byte[] GetCount = Packet("cou2");
        public static readonly byte[] Count = Packet("siz2");
        public static readonly byte[] GetInfo = Packet("gie3");
        public static readonly byte[] Info = Packet("inf3");
        public static readonly byte[] GetInfo64Legacy = Packet("fstd");
        public static readonly byte[] Info64Legacy = Packet("dtsf");
        public static readonly byte[] InfoExtended = Packet("iext");
        public static readonly byte[] InfoExtendedMore = Packet("iex+");
        public static readonly byte[] FirewallCheck = Packet("fw??");
        public static readonly byte[] FirewallResponse = Packet("fw!!");
        public static readonly byte[] FirewallOk = Packet("fwok");
        public static readonly byte[] FirewallError = Packet("fwer");

        private static byte[] Packet(string symbols)
        {
            var buffer = new Span<byte>(new byte[8]);
            buffer.Slice(0, 4).Fill(255);
            buffer[4] = (byte) symbols[0];
            buffer[5] = (byte) symbols[1];
            buffer[6] = (byte) symbols[2];
            buffer[7] = (byte) symbols[3];
            return buffer.ToArray();
        }
    }
}
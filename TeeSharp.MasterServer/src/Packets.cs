using System;

namespace TeeSharp.MasterServer
{
    public readonly ref struct Packets
    {
        public static readonly byte[] Heartbeat = Packet('b', 'e', 'a', '2');
        public static readonly byte[] GetList = Packet('r', 'e', 'q', '2');
        public static readonly byte[] List = Packet('l', 'i', 's', '2');
        public static readonly byte[] GetCount = Packet('c', 'o', 'u', '2');
        public static readonly byte[] Count = Packet('s', 'i', 'z', '2');
        public static readonly byte[] GetInfo = Packet('g', 'i', 'e', '3');
        public static readonly byte[] Info = Packet('i', 'n', 'f', '3');
        public static readonly byte[] GetInfo64Legacy = Packet('f', 's', 't', 'd');
        public static readonly byte[] Info64Legacy = Packet('d', 't', 's', 'f');
        public static readonly byte[] InfoExtended = Packet('i', 'e', 'x', 't');
        public static readonly byte[] InfoExtendedMore = Packet('i', 'e', 'x', '+');
        public static readonly byte[] FirewallCheck = Packet('f', 'w', '?', '?');
        public static readonly byte[] FirewallResponse = Packet('f', 'w', '!', '!');
        public static readonly byte[] FirewallOk = Packet('f', 'w', 'o', 'k');
        public static readonly byte[] FirewallError = Packet('f', 'w', 'e', 'r');

        private static byte[] Packet(char b1, char b2, char b3, char b4)
        {
            return new byte[] {255, 255, 255, 255, (byte) b1, (byte) b2, (byte) b3, (byte) b4};
        }
    }
}
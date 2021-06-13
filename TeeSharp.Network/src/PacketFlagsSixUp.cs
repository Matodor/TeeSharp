using System;

namespace TeeSharp.Network
{
    [Flags]
    public enum PacketFlagsSixUp
    {
        None           = 0,
        Connection     = 1 << 0,
        Resend         = 1 << 1,
        Compression    = 1 << 2,
        ConnectionLess = 1 << 3,
    }
}
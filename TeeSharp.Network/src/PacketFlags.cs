using System;

namespace TeeSharp.Network
{
    [Flags]
    public enum PacketFlags
    {
        None            = 0,
        Unused          = 1 << 0,
        Token           = 1 << 1,
        ConnectionState = 1 << 2,
        ConnectionLess  = 1 << 3,
        Resend          = 1 << 4,
        Compression     = 1 << 5,
        Extended        = 1 << 6,
    }
}
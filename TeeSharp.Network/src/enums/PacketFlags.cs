using System;

namespace TeeSharp.Network.Enums
{
    [Flags]
    public enum PacketFlags
    {
        CONTROL = 1 << 0,
        CONNLESS = 1 << 1,
        RESEND = 1 << 2,
        COMPRESSION = 1 << 3
    }
}
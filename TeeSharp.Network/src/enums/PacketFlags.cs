using System;

namespace TeeSharp.Network.Enums
{
    [Flags]
    public enum PacketFlags
    {
        NONE = 0,
        UNUSED = 1 << 0,
        TOKEN = 1 << 1,
        CONTROL = 1 << 2,
        CONNLESS = 1 << 3,
        RESEND = 1 << 4,
        COMPRESSION = 1 << 5,
    }
}
using System;

namespace TeeSharp.Network.Enums
{
    [Flags]
    public enum PacketFlags
    {
        None = 0,
        Control = 1,
        Resend = 2,
        Compression = 4,
        Connless = 8
    }
}
using System;

namespace TeeSharp.Network.Enums
{
    [Flags]
    public enum SendFlags
    {
        NONE = 0,
        VITAL = 1 << 0,
        CONNLESS = 1 << 1,
        FLUSH = 1 << 2,
    }
}
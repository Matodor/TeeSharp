using System;

namespace TeeSharp.Network.Enums
{
    [Flags]
    public enum SendFlags
    {
        None = 0,
        Vital = 1,
        Connless = 2,
        Flush = 4,
    }
}
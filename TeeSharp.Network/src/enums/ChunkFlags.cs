using System;

namespace TeeSharp.Network.Enums
{
    [Flags]
    public enum ChunkFlags
    {
        None = 0,
        Vital = 1,
        Resend = 2
    }
}
using System;

namespace TeeSharp.Network.Enums
{
    [Flags]
    public enum ChunkFlags
    {
        NONE = 0,
        VITAL = 1 << 0,
        RESEND = 1 << 1
    }
}
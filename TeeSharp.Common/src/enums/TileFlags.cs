using System;

namespace TeeSharp.Common.Enums
{
    [Flags]
    public enum TileFlags
    {
        None = 0,
        Solid = 1 << 0,
        Death = 1 << 1,
        NoHook = 1 << 2
    }
}
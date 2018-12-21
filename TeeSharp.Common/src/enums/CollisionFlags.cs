using System;

namespace TeeSharp.Common.Enums
{
    [Flags]
    public enum CollisionFlags
    {
        None = 0,
        Solid = 1 << 0,
        Death = 1 << 1,
        NoHook = 1 << 2
    }
}
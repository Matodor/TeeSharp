using System;

namespace TeeSharp.Common.Enums
{
    [Flags]
    public enum CoreEvents
    {
        None = 0,
        GroundJump = 1 << 0,
        AirJump = 1 << 1,
        HookAttachPlayer = 1 << 2,
        HookAttachGround = 1 << 3,
        HookHitNoHook = 1 << 4,
    }
}
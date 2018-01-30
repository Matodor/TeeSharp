using System;

namespace TeeSharp.Common.Enums
{
    [Flags]
    public enum CoreEvents
    {
        NONE = 0,
        GROUND_JUMP = 1 << 0,
        AIR_JUMP = 1 << 1,
        HOOK_LAUNCH = 1 << 2,
        HOOK_ATTACH_PLAYER = 1 << 3,
        HOOK_ATTACH_GROUND = 1 << 4,
        HOOK_HIT_NOHOOK = 1 << 5,
        HOOK_RETRACT = 1 << 6,
    }
}
using System;

namespace TeeSharp.Common.Enums
{
    [Flags]
    public enum PlayerFlags
    {
        PLAYING = 1 << 0,
        PLAYERFLAG_IN_MENU = 1 << 1,
        PLAYERFLAG_CHATTING = 1 << 2,
        PLAYERFLAG_SCOREBOARD = 1 << 3,
    }
}
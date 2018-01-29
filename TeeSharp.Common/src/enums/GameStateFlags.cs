using System;

namespace TeeSharp.Common.Enums
{
    [Flags]
    public enum GameStateFlags
    {
        NONE = 0,
        GAMEOVER = 1 << 0,
        SUDDENDEATH = 1 << 1,
        PAUSED = 1 << 2,
    }
}
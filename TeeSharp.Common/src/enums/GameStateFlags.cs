using System;

namespace TeeSharp.Common.Enums
{
    [Flags]
    public enum GameStateFlags
    {
        GAMEOVER = 1 << 0,
        SUDDENDEATH = 1 << 1,
        PAUSED = 1 << 2,
    }
}
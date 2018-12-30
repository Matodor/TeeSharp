using System;

namespace TeeSharp.Common.Enums
{
    [Flags]
    public enum GameStateFlags
    {
        None = 0,
        Warmup = 1 << 0,
        SuddenDeath = 1 << 1,
        RoundOver = 1 << 2,
        GameOver = 1 << 3,
        Paused = 1 << 4,
        StartCountDown = 1 << 5
    }
}
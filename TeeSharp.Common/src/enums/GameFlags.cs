using System;

namespace TeeSharp.Common.Enums
{
    [Flags]
    public enum GameFlags
    {
        TEAMS = 1 << 0,
        FLAGS = 1 << 1,
    }
}
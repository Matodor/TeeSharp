using System;

namespace TeeSharp.Common.Enums
{
    [Flags]
    public enum GameFlags
    {
        None = 0,
        Teams = 1 << 0,
        Flags = 1 << 1,
        Survival = 1 << 2,
    }
}
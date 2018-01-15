using System;

namespace TeeSharp.Common.Config
{
    [Flags]
    public enum ConfigFlags
    {
        SAVE = 1 << 0,
        CLIENT = 1 << 1,
        SERVER = 1 << 2,
        STORE = 1 << 3,
        MASTER = 1 << 4,
        ECON = 1 << 5,
    }
}
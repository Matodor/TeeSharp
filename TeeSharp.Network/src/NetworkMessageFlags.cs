using System;

namespace TeeSharp.Network;

[Flags]
public enum NetworkMessageFlags
{
    None           = 0,
    Vital          = 1 << 0,
    ConnectionLess = 1 << 1,
    // Flush          = 1 << 2,
}

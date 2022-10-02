using System;

namespace TeeSharp.Network;

[Flags]
public enum NetworkSendFlags
{
    None           = 0,
    Vital          = 1 << 0,
    ConnectionLess = 1 << 1,
    Flush          = 1 << 2,
    Extended       = 1 << 3,
}

using System;

namespace TeeSharp.Network;

[Flags]
public enum NetworkMessageHeaderFlags
{
    None = 0,
    Vital = 1 << 0,
}

using System;

namespace TeeSharp.Network.Enums
{
    [Flags]
    public enum TokenFlags
    {
        None = 0,
        AllowBroadcast = 1,
        ResponseOnly = 2,
    }
}
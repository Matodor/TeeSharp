using System;

namespace TeeSharp.Common.Enums
{
    [Flags]
    public enum MsgFlags
    {
        NONE = 0,
        VITAL = 1 << 0,
        FLUSH = 1 << 1,
        NORECORD = 1 << 2,
        RECORD = 1 << 3,
        NOSEND = 1 << 4,
    }
}
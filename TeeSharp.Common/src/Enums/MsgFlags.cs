using System;

namespace TeeSharp.Common.Enums
{
    [Flags]
    public enum MsgFlags
    {
        None = 0,
        Vital = 1 << 0,
        Flush = 1 << 1,
        NoRecord = 1 << 2,
        Record = 1 << 3,
        NoSend = 1 << 4,
    }
}
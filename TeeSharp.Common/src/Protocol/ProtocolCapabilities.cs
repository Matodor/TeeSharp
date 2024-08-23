using System;
using System.Diagnostics.CodeAnalysis;

namespace TeeSharp.Common;

[SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public partial class Protocol
{
    [Flags]
    public enum Capabilities
    {
        CurrentVersion = 5,

        DDNet = 1 << 0,
        ChatTimeoutCode = 1 << 1,
        AnyPlayerFlag = 1 << 2,
        PingExtended = 1 << 3,
        AllowDummy = 1 << 4,
        SyncWeaponInput = 1 << 5,
    }
}

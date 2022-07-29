using System.Diagnostics.CodeAnalysis;

namespace TeeSharp.Common.Protocol;

public enum ProtocolCapabilities
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    DDNet = 1 << 0,
    ChatTimeoutCode = 1 << 1,
    AnyPlayerFlag = 1 << 2,
    PingExtended = 1 << 3,
    AllowDummy = 1 << 4,
    SyncWeaponInput = 1 << 5,
}

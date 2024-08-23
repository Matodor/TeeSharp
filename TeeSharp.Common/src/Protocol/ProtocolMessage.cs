using System.Diagnostics.CodeAnalysis;

namespace TeeSharp.Common;

[SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public partial class Protocol
{
    public enum Message
    {
        Empty = 0,
        ClientInfo,

        ServerMapChange,
        ServerMapData,
        ServerConnectionReady,
        ServerSnap,
        ServerSnapEmpty,
        ServerSnapSingle,
        ServerSnapSmall,
        ServerInputTiming,
        ServerRconAuthStatus,
        ServerRconLine,
        ServerAuthChallenge,
        ServerAuthResult,

        ClientReady,
        ClientEnterGame,
        ClientInput,
        ClientRconCommand,
        ClientRconAuth,
        ClientRequestMapData,
        ClientAuthStart,
        ClientAuthResponse,

        Ping,
        PingReply,
        Error,

        ServerRconCommandAdd,
        ServerRconCommandRemove,
    }
}

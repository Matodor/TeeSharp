namespace TeeSharp.Common.Protocol;

public enum ProtocolMessage
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

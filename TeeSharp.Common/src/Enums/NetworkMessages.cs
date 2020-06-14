namespace TeeSharp.Common.Enums
{
    public enum NetworkMessages
    {
        Null = 0,

        ClientInfo = 1,

        ServerMapChange,
        ServerMapData,
        ServerInfo,
        ServerConnectionReady,
        ServerSnap,
        ServerSnapEmpty,
        ServerSnapSingle,
        ServerSnapSmall,
        ServerInputTiming,
        ServerRconAuthOn,
        ServerRconAuthOff,
        ServerRconLine,
        ServerRconCommandAdd,
        ServerRconCommandRemove,
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
    }
}
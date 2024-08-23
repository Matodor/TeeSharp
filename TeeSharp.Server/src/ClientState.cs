namespace TeeSharp.Server;

public enum ClientState
{
    Empty,
    PreAuth,
    Auth,
    Connecting,
    Ready,
    InGame,
}

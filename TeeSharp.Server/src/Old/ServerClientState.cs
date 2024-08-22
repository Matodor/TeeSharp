namespace TeeSharp.Server.Old;

public enum ServerClientState
{
    Empty,
    PreAuth,
    Auth,
    Connecting,
    Ready,
    InGame,
}

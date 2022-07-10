namespace TeeSharp.Server;

public enum ServerClientState
{
    Empty,
    PreAuth,
    Auth,
    Connecting,
    Ready,
    InGame,
}

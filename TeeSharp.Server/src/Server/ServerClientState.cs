namespace TeeSharp.Server
{
    public enum ServerClientState
    {
        Empty = 0,
        Auth,
        Connecting,
        Ready,
        InGame,
    }
}
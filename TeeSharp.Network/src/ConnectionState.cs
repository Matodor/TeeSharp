namespace TeeSharp.Network;

public enum ConnectionState
{
    Connecting,
    Pending,
    Online,
    Timeout,
    Disconnecting,
    Offline,
}

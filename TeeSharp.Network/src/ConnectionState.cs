namespace TeeSharp.Network;

public enum ConnectionState
{
    Offline = 0,
    Pending,
    Online,
    Disconnecting,
    Timeout,
}

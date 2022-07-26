namespace TeeSharp.Network;

public enum ConnectionState
{
    Offline = 0,
    Connect,
    Pending,
    Online,
    Disconnecting,
    Timeout,
}

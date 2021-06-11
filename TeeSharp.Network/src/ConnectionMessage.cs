namespace TeeSharp.Network
{
    public enum ConnectionMessage
    {
        KeepAlive = 0,
        Connect,
        ConnectAccept,
        Accept,
        Close,
    }
}
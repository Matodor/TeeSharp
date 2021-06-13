namespace TeeSharp.Network
{
    public enum ConnectionStateMsg
    {
        KeepAlive = 0,
        Connect,
        ConnectAccept,
        Accept,
        Close,
    }
}
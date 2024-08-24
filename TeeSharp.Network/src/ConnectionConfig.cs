namespace TeeSharp.Network;

public class ConnectionConfig
{
    /// <summary>
    /// Time in seconds after which the connection is considered timeouted
    /// </summary>
    public int Timeout { get; set; } = 100;
}

namespace TeeSharp.Network;

public class ConnectionSettings
{
    /// <summary>
    /// Time in seconds after which the connection is considered timeouted
    /// </summary>
    public int Timeout { get; set; } = 10;
}

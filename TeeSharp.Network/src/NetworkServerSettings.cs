namespace TeeSharp.Network;

public class NetworkServerSettings
{
    public ushort Port { get; set; } = 8303;
    public int MaxConnections { get; set; } = 64;
    public int MaxConnectionsPerIp { get; set; } = 4;
}

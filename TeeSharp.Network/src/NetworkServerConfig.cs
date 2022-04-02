namespace TeeSharp.Network;

public struct NetworkServerConfig
{
    public int MaxConnections { get; set; }
    public int MaxConnectionsPerIp { get; set; }
}
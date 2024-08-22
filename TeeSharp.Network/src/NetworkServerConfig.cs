namespace TeeSharp.Network;

public class NetworkServerConfig
{
    /// <summary>
    /// TODO
    /// </summary>
    public int Port { get; set; } = 8303;

    /// <summary>
    ///  TODO
    /// </summary>
    public string BindAddress { get; set; } = string.Empty;

    /// <summary>
    /// TODO
    /// </summary>
    public int MaxConnections { get; set; } = 64;

    /// <summary>
    /// TODO
    /// </summary>
    public int MaxConnectionsPerIp { get; set; } = 4;

    /// <summary>
    /// TODO
    /// </summary>
    public ConnectionConfig Connection { get; set; } = new();
}

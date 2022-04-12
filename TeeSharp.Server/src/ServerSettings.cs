using System;

namespace TeeSharp.Server;

public class ServerSettings
{
    public bool UseHotReload = true;

    public string Name = "[TeeSharp] Unnamed server";

    public ushort Port { get; set; }
    public string BindAddress { get; set; } = string.Empty;

    public int MaxConnections { get; set; } = 64;
    public int MaxConnectionsPerIp { get; set; } = 4;
}

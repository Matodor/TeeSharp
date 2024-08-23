using TeeSharp.Network;

namespace TeeSharp.Server;

public class ServerConfig
{
    public string Name { get; set; } = "[TeeSharp] Unnamed server";
    public int TickRate { get; set; } = 50;
    public bool HighBandwidth { get; set; } = false;

    public NetworkServerConfig Network { get; set; } = new();
}

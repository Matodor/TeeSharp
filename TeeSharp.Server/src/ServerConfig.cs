using TeeSharp.Network;

namespace TeeSharp.Server;

public class ServerConfig
{
    public string Name { get; set; } = "[TeeSharp] Unnamed server";
    public string Password { get; set; } = string.Empty;
    public int TickRate { get; set; } = 50;
    public int ReservedSlots { get; set; } = 0;
    public bool HighBandwidth { get; set; } = false;
    public bool AllowDummy { get; set; } = true;
    public NetworkServerConfig Network { get; set; } = new();
}

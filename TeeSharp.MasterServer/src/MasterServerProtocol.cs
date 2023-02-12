namespace TeeSharp.MasterServer;

public class MasterServerProtocol
{
    public MasterServerProtocolType Type { get; }
    public bool Enabled { get; set; }

    public MasterServerProtocol(MasterServerProtocolType type)
    {
        Type = type;
    }
}

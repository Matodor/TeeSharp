namespace TeeSharp.Network;

public class NetworkMessageHeader
{
    public NetworkMessageFlags Flags { get; private set; }
    public int Size { get; private set; }
    public int Sequence { get; private set; }

    public NetworkMessageHeader()
    {
        Flags = NetworkMessageFlags.None;
        Size = 0;
        Sequence = 0;
    }
}

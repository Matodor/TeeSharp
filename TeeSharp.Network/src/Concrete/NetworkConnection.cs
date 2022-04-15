using TeeSharp.Network.Abstract;

namespace TeeSharp.Network.Concrete;

public class NetworkConnection : INetworkConnection
{
    public int Id { get; }
    public bool IsSixUp { get; set; }

    public NetworkConnection(int id)
    {
        Id = id;
    }
}

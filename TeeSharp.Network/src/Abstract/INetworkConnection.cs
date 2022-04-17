namespace TeeSharp.Network.Abstract;

public interface INetworkConnection
{
    int Id { get; }
    bool IsSixup { get; set; }
}

namespace TeeSharp.Network.Abstract;

public interface INetworkConnection
{
    int Id { get; }
    bool IsSixUp { get; set; }
}

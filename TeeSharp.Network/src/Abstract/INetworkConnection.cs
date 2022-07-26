using System.Collections.Generic;
using System.Net;

namespace TeeSharp.Network.Abstract;

public interface INetworkConnection
{
    int Id { get; }
    ConnectionState State { get; }
    IPEndPoint EndPoint { get; }

    void Init(IPEndPoint endPoint, SecurityToken securityToken);
    void Disconnect(string reason);
    IEnumerable<NetworkMessage> ProcessPacket(IPEndPoint endPoint, NetworkPacketIn packet);
    void Update();
}

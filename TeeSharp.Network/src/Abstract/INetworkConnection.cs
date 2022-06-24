using System.Collections.Generic;
using System.Net;

namespace TeeSharp.Network.Abstract;

public interface INetworkConnection
{
    int Id { get; }
    ConnectionState State { get; }
    IPEndPoint EndPoint { get; }

    void Init(int id, IPEndPoint endPoint, SecurityToken securityToken);
    IEnumerable<NetworkMessage> ProcessPacket(IPEndPoint endPoint, NetworkPacketIn packet);
}

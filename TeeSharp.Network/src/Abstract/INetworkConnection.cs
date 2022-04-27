using System.Collections.Generic;
using System.Net;

namespace TeeSharp.Network.Abstract;

public interface INetworkConnection
{
    ConnectionState State { get; }

    IPEndPoint EndPoint { get; }

    bool IsSixup { get; }

    void Init(IPEndPoint endPoint, SecurityToken securityToken, bool isSixup);
    IEnumerable<NetworkMessage> ProcessPacket(NetworkPacketIn packet);
}

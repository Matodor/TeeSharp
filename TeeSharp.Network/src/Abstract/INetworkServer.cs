using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace TeeSharp.Network.Abstract;

public interface INetworkServer : IDisposable
{
    event Action<INetworkConnection> ConnectionAccepted;

    int MaxConnections { get; set; }
    int MaxConnectionsPerIp { get; set; }
    bool AcceptSixupConnections { get; set; }

    INetworkPacketUnpacker PacketUnpacker { get; }
    IReadOnlyList<INetworkConnection> Connections { get; }

    bool TryInit(
        IPEndPoint localEP,
        int maxConnections = 64,
        int maxConnectionsPerIp = 4,
        bool acceptSixupConnections = true);

    bool TryGetConnectionId(IPEndPoint endPoint, out int id);

    IEnumerable<NetworkMessage> GetMessages(CancellationToken cancellationToken);
}

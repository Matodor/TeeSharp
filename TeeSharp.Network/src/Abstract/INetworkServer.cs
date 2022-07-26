using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace TeeSharp.Network.Abstract;

public interface INetworkServer : IDisposable
{
    event Action<INetworkConnection> ConnectionAccepted;

    ConnectionSettings ConnectionSettings { get; }
    int MaxConnections { get; }
    int MaxConnectionsPerIp { get; set; }

    INetworkPacketUnpacker PacketUnpacker { get; }
    IReadOnlyList<INetworkConnection> Connections { get; }

    bool TryInit(
        IPEndPoint localEP,
        int maxConnections = 64,
        int maxConnectionsPerIp = 4,
        ConnectionSettings? connectionSettings = null);

    bool TryGetConnectionId(IPEndPoint endPoint, out int id);
    IEnumerable<NetworkMessage> GetMessages(CancellationToken cancellationToken);
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;

namespace TeeSharp.Network.Abstract;

public interface INetworkServer : IDisposable
{
    int MaxConnections { get; set; }
    int MaxConnectionsPerIp { get; set; }

    INetworkPacketUnpacker PacketUnpacker { get; }
    IReadOnlyList<INetworkConnection> Connections { get; }

    bool TryInit(
        IPEndPoint localEP,
        int maxConnections = 64,
        int maxConnectionsPerIp = 4);

    bool TryGetConnectionId(IPEndPoint endPoint, out int id);

    IEnumerable<NetworkMessage> GetMessages(CancellationToken cancellationToken);
}

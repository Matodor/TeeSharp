using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace TeeSharp.Network.Abstract;

public interface INetworkServer : IDisposable
{
    event Action<INetworkConnection> ConnectionAccepted;
    event Action<INetworkConnection, string> ConnectionDropped;

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
    void Update();
    void Send(int connectionId, Span<byte> data, NetworkSendFlags sendFlags);
    void SendData(IPEndPoint endPoint, ReadOnlySpan<byte> data, ReadOnlySpan<byte> extraData = default);
    void Drop(int connectionId, string reason);
}

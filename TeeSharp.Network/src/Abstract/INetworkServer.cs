using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;

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

    bool TryReceive(
        [NotNullWhen(true)] out NetworkMessage? message,
        [NotNullWhen(true)] out SecurityToken? responseToken);

    bool TryGetConnection(
        IPEndPoint endPoint,
        [NotNullWhen(true)] out INetworkConnection? connection);
}

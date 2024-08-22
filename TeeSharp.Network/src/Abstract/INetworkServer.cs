using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;

namespace TeeSharp.Network.Abstract;

public interface INetworkServer : IDisposable
{
    event Action<INetworkConnection> ConnectionAccepted;
    event Action<INetworkConnection, string> ConnectionDropped;

    NetworkServerConfig Config { get; }
    INetworkPacketUnpacker PacketUnpacker { get; }
    IReadOnlyList<INetworkConnection> Connections { get; }

    void Init(NetworkServerConfig config);
    bool TryGetLocalEndPoint([NotNullWhen(true)] out EndPoint? localEndPoint);
    bool TryGetConnectionId(IPEndPoint endPoint, out int id);
    IEnumerable<NetworkMessage> GetMessages(CancellationToken cancellationToken);
    void Update();
    void Send(int connectionId, Span<byte> data, NetworkSendFlags sendFlags);
    void SendData(IPEndPoint endPoint, ReadOnlySpan<byte> data, ReadOnlySpan<byte> extraData = default);
    void Drop(int connectionId, string reason);
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace TeeSharp.Network.Abstract;

public interface INetworkServer : IDisposable
{
    INetworkPacketUnpacker PacketUnpacker { get; }
    IReadOnlyList<INetworkConnection> Connections { get; }

    bool TryInit(IPEndPoint bind);

    bool TryReceive([NotNullWhen(true)] out SecurityToken responseToken);
}

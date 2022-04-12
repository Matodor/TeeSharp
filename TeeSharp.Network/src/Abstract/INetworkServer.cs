using System;
using System.Collections.Generic;
using System.Net;

namespace TeeSharp.Network.Abstract;

public interface INetworkServer : IDisposable
{
    IReadOnlyList<INetworkConnection> Connections { get; }

    bool TryInit(IPEndPoint bind);
}

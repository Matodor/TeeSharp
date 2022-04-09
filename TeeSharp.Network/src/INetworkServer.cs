using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace TeeSharp.Network;

public interface INetworkServer
{
    IReadOnlyList<INetworkConnection> Connections { get; }

    Task RunAsync(CancellationToken cancellationToken);
}

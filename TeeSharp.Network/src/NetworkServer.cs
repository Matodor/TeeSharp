using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TeeSharp.Network;

public class NetworkServer : INetworkServer
{
    public IReadOnlyList<INetworkConnection> Connections { get; }

    private readonly ILogger _logger;

    public NetworkServer(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger("GameServer");
    }

    public virtual async Task RunAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }
}

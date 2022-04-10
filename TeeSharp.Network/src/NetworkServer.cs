using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeeSharp.Core;
using TeeSharp.Core.Settings;

namespace TeeSharp.Network;

public class NetworkServer : INetworkServer
{
    public IReadOnlyList<INetworkConnection> Connections { get; }

    private readonly ILogger _logger;

    public NetworkServer(
        ISettingsChangesNotifier<NetworkServerSettings> settingsChangesNotifier)
    {
        _logger = Tee.LoggerFactory.CreateLogger("Network");
    }

    public virtual async Task RunAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }
}

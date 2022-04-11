using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeeSharp.Core;
using TeeSharp.Core.Settings;

namespace TeeSharp.Network;

public class NetworkServer : INetworkServer
{
    public IReadOnlyList<INetworkConnection> Connections { get; protected set; }

    protected ILogger Logger { get; set; }

    public NetworkServer(
        ISettingsChangesNotifier<NetworkServerSettings> settingsChangesNotifier)
    {
        Logger = Tee.LoggerFactory.CreateLogger("Network");
    }

    public virtual async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (TaskCanceledException e)
        {
            // ignore
        }

        Logger.LogDebug("Network server stopped");
    }
}

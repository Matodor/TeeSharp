using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TeeSharp.Core;
using TeeSharp.Network.Abstract;

namespace TeeSharp.Network.Concrete;

public class NetworkServer : INetworkServer
{
    public IReadOnlyList<INetworkConnection> Connections { get; protected set; }

    protected EndPoint? EndPoint => Socket?.Client.LocalEndPoint;
    protected UdpClient? Socket { get; set; }

    protected ILogger Logger { get; set; }

    public NetworkServer(ILogger? logger = null)
    {
        Logger = logger ?? Tee.LoggerFactory.CreateLogger("NetworkServer");
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public virtual bool TryInit(IPEndPoint localEP)
    {
        if (!NetworkHelper.TryGetUdpClient(localEP, out var socket))
        {
            Logger.LogError("Error creating UdpClient for endpoint: {LocalEP}", localEP);
            return false;
        }

        Socket = socket;
        Socket.Client.Blocking = true;

        Logger.LogDebug("The network server has been successfully initialized");
        Logger.LogInformation("Local address: {EndPoint}", EndPoint);
        return true;
    }

    public void Dispose()
    {
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeeSharp.Core;
using TeeSharp.Network.Abstract;

namespace TeeSharp.Network.Concrete;

public class NetworkServer : INetworkServer
{
    public INetworkPacketUnpacker PacketUnpacker { get; protected set; }
    public IReadOnlyList<INetworkConnection> Connections { get; protected set; }

    protected EndPoint? EndPoint => Socket?.Client.LocalEndPoint;
    protected UdpClient? Socket { get; set; }

    protected ILogger Logger { get; set; }

    public NetworkServer(ILogger? logger = null)
    {
        Logger = logger ?? Tee.LoggerFactory.CreateLogger("NetworkServer");
        PacketUnpacker = CreatePacketUnpacker();
    }

    protected virtual INetworkPacketUnpacker CreatePacketUnpacker()
    {
        return new NetworkPacketUnpackerSixup();
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
        Socket.Client.ReceiveTimeout = 1000;

        Logger.LogDebug("The network server has been successfully initialized");
        Logger.LogInformation("Local address: {EndPoint}", EndPoint!.ToString());
        return true;
    }

    public bool TryReceive(out SecurityToken responseToken)
    {
        Span<byte> data;
        var endPoint = default(IPEndPoint);

        try
        {
            data = Socket!.Receive(ref endPoint).AsSpan();
        }
        catch (SocketException e)
        {
            if (e.ErrorCode != (int) SocketError.TimedOut)
                throw;

            responseToken = default;
            return false;
        }

        if (data.Length == 0)
        {
            responseToken = default;
            return false;
        }

        var isSixUp = false;
        var securityToken = default(SecurityToken);

        responseToken = SecurityToken.Unknown;

        if (!PacketUnpacker.TryUnpack(
            data,
            out var networkPacket,
            ref isSixUp,
            ref securityToken,
            ref responseToken))
        {
            return false;
        }

        return true;
    }

    public void Dispose()
    {
        Socket?.Close();
        Socket?.Dispose();
        Socket = null;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeeSharp.Core;
using TeeSharp.Core.Helpers;
using TeeSharp.Network.Abstract;

namespace TeeSharp.Network.Concrete;

public class NetworkServer : INetworkServer
{
    public int MaxConnections { get; set; }
    public int MaxConnectionsPerIp { get; set; }
    public INetworkPacketUnpacker PacketUnpacker { get; protected set; } = null!;
    public IReadOnlyList<INetworkConnection> Connections { get; protected set; } = null!;

    protected Dictionary<int, int> MapClients { get; set; } = null!;
    protected EndPoint? EndPoint => Socket?.Client.LocalEndPoint;
    protected UdpClient? Socket { get; set; }
    protected ILogger Logger { get; set; }
    protected byte[] SecurityTokenSeed { get; set; } = null!;

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
    public virtual bool TryInit(
        IPEndPoint localEP,
        int maxConnections = 64,
        int maxConnectionsPerIp = 4)
    {
        if (!NetworkHelper.TryGetUdpClient(localEP, out var socket))
        {
            Logger.LogError("Error creating UdpClient for endpoint: {LocalEP}", localEP);
            return false;
        }

        Socket = socket;
        Socket.Client.Blocking = true;
        Socket.Client.ReceiveTimeout = 10;

        MaxConnections = maxConnections;
        MaxConnectionsPerIp = maxConnectionsPerIp;

        Logger.LogDebug("The network server has been successfully initialized");
        Logger.LogInformation("Local address: {EndPoint}", EndPoint!.ToString());

        MapClients = new Dictionary<int, int>(MaxConnections);
        Connections = Enumerable.Range(0, MaxConnections)
            .Select(CreateEmptyConnection)
            .ToArray();

        RefreshSecurityTokenSeed();

        return true;
    }

    protected virtual INetworkConnection CreateEmptyConnection(int id)
    {
        return new NetworkConnection(id);
    }

    public bool TryGetConnection(
        IPEndPoint endPoint,
        [NotNullWhen(true)] out INetworkConnection? connection)
    {
        if (MapClients.TryGetValue(endPoint.GetHashCode(), out var id))
        {
            connection = Connections[id];
            return true;
        }

        connection = null;
        return false;
    }

    public IEnumerable<NetworkMessage> GetMessages(CancellationToken cancellationToken)
    {
        var endPoint = default(IPEndPoint);

        while (!cancellationToken.IsCancellationRequested &&
            Socket!.Available > 0)
        {
            Span<byte> data;

            try
            {
                data = Socket!.Receive(ref endPoint).AsSpan();
            }
            catch (SocketException e)
            {
                if (e.ErrorCode != (int)SocketError.TimedOut)
                {
                    throw;
                }

                yield break;
            }

            if (!PacketUnpacker.TryUnpack(data, out var packet))
            {
                yield break;
            }

            SecurityToken securityToken = packet.SecurityToken ?? default;
            SecurityToken responseToken = packet.ResponseToken ?? SecurityToken.Unknown;

            if (packet.Flags.HasFlag(PacketFlags.ConnectionLess))
            {
                if (packet.IsSixup && packet.SecurityToken != GetToken(endPoint))
                {
                    yield break;
                }

                yield return new NetworkMessage(
                    connectionId: -1,
                    endPoint: endPoint,
                    flags: NetworkMessageFlags.ConnectionLess,
                    data: packet.Data,
                    extraData: packet.ExtraData
                );

                yield break;
            }

            if (packet.Data.Length == 0 &&
                packet.Flags.HasFlag(PacketFlags.ConnectionState))
            {
                yield break;
            }

            if (TryGetConnection(endPoint, out var connection))
            {
                if (!packet.IsSixup && connection.IsSixUp)
                {
                    throw new NotImplementedException();
                }

                throw new NotImplementedException();
            }
            else
            {
                if (packet.IsSixup)
                {
                    throw new NotImplementedException();
                }

                // if (IsConnStateMsgWithToken(packet))
                // {
                //     ProcessConnStateMsgWithToken(endPoint, packet);
                //     return false;
                // }

                throw new NotImplementedException();
            }
        }
    }

    protected virtual SecurityToken GetToken(IPEndPoint endPoint)
    {
        const int offset = sizeof(int);
        var buffer = (Span<byte>) new byte[offset + SecurityTokenSeed.Length];
        Unsafe.As<byte, int>(ref buffer[0]) = endPoint.GetHashCode();
        SecurityTokenSeed.CopyTo(buffer.Slice(offset));

        return SecurityHelper.KnuthHash(buffer).GetHashCode();
    }

    protected virtual void RefreshSecurityTokenSeed()
    {
        SecurityTokenSeed = new byte[12];
        RandomNumberGenerator.Create().GetBytes(SecurityTokenSeed);
    }

    public void Dispose()
    {
        Socket?.Close();
        Socket?.Dispose();
        Socket = null;
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
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

    public bool TryGetConnectionId(IPEndPoint endPoint, out int id)
    {
        return MapClients.TryGetValue(endPoint.GetHashCode(), out id);
    }

    public IEnumerable<NetworkMessage> GetMessages(CancellationToken cancellationToken)
    {
        var endPoint = default(IPEndPoint);

        while (!cancellationToken.IsCancellationRequested && Socket!.Available > 0)
        {
            Span<byte> data;

            try
            {
                data = Socket!.Receive(ref endPoint).AsSpan();
            }
            catch (SocketException e)
            {
                if (e.ErrorCode != (int) SocketError.TimedOut)
                {
                    throw;
                }

                continue;
            }

            if (!PacketUnpacker.TryUnpack(data, out var packet))
            {
                continue;
            }

            if (packet.Flags.HasFlag(PacketFlags.ConnectionLess))
            {
                if (packet.IsSixup && packet.SecurityToken != GetToken(endPoint))
                {
                    continue;
                }

                yield return new NetworkMessage(
                    connectionId: -1,
                    endPoint: endPoint,
                    flags: NetworkMessageFlags.ConnectionLess,
                    responseToken: packet.ResponseToken,
                    data: packet.Data,
                    extraData: packet.ExtraData
                );

                continue;
            }

            if (packet.Data.Length == 0 && packet.Flags.HasFlag(PacketFlags.Connection))
            {
                continue;
            }

            if (TryGetConnectionId(endPoint, out var connectionId))
            {
                if (!packet.IsSixup && Connections[connectionId].IsSixup)
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
                else
                {
                    ProcessConnectionStateMessage(endPoint, packet);
                }
            }
        }
    }

    protected virtual void ProcessConnectionStateMessage(
        IPEndPoint endPoint,
        NetworkPacketIn packetIn)
    {
        if (packetIn.Data.Length == 0 || !packetIn.Flags.HasFlag(PacketFlags.Connection))
        {
            return;
        }

        switch ((ConnectionStateMsg) packetIn.Data[0])
        {
            case ConnectionStateMsg.Connect:
                if (packetIn.Data.Length >= 1 + StructHelper<SecurityToken>.Size * 2
                    && packetIn.Data.AsSpan(1, StructHelper<SecurityToken>.Size) == SecurityToken.Magic)
                {
                    SendConnectionStateMsg(
                        endPoint: endPoint,
                        msg: ConnectionStateMsg.ConnectAccept,
                        token: GetToken(endPoint),
                        isSixup: packetIn.IsSixup,
                        extraData: SecurityToken.Magic
                    );
                }

                break;

            case ConnectionStateMsg.Accept:
                if (packetIn.Data.Length >= 1 + StructHelper<SecurityToken>.Size)
                {
                    return;
                }

                break;
        }
    }

    protected virtual void SendConnectionStateMsg(
        IPEndPoint endPoint,
        ConnectionStateMsg msg,
        SecurityToken token,
        bool isSixup,
        string? extraMsg = null)
    {
        NetworkHelper.SendConnectionStateMsg(
            Socket!,
            endPoint,
            msg,
            token,
            0,
            isSixup,
            extraMsg
        );
    }

    protected virtual void SendConnectionStateMsg(
        IPEndPoint endPoint,
        ConnectionStateMsg msg,
        SecurityToken token,
        bool isSixup,
        byte[] extraData)
    {
        NetworkHelper.SendConnectionStateMsg(
            Socket!,
            endPoint,
            msg,
            token,
            0,
            isSixup,
            extraData
        );
    }

    protected virtual SecurityToken GetToken(IPEndPoint endPoint)
    {
        const int offset = sizeof(int);
        var buffer = (Span<byte>) new byte[offset + SecurityTokenSeed.Length];
        Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(buffer), endPoint.GetHashCode());
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

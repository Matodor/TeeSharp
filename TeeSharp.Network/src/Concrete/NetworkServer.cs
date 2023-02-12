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
    public event Action<INetworkConnection> ConnectionAccepted = delegate {  };
    public event Action<INetworkConnection, string>? ConnectionDropped;

    public ConnectionSettings ConnectionSettings { get; private set; } = null!;
    public int MaxConnections { get; private set; }
    public int MaxConnectionsPerIp { get; set; }
    public INetworkPacketUnpacker PacketUnpacker { get; protected set; }
    public IReadOnlyList<INetworkConnection> Connections { get; protected set; } = null!;

    protected Dictionary<int, int> MapConnections { get; set; } = null!;
    protected EndPoint EndPoint => Socket.Client.LocalEndPoint!;
    protected UdpClient Socket { get; set; } = null!;
    protected ILogger Logger { get; set; }
    protected byte[] SecurityTokenSeed { get; set; } = null!;

    public NetworkServer(ILogger? logger = null)
    {
        Logger = logger ?? Tee.LoggerFactory.CreateLogger("NetworkServer");
        PacketUnpacker = CreatePacketUnpacker();
    }

    protected virtual INetworkPacketUnpacker CreatePacketUnpacker()
    {
        return new NetworkPacketUnpacker();
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public virtual bool TryInit(
        IPEndPoint localEP,
        int maxConnections = 64,
        int maxConnectionsPerIp = 4,
        ConnectionSettings? connectionSettings = null)
    {
        if (!NetworkHelper.TryGetUdpClient(localEP, out var socket))
        {
            Logger.LogError("Error creating UdpClient for endpoint: {LocalEP}", localEP);
            return false;
        }

        Socket = socket;
        Socket.Client.Blocking = true;
        Socket.Client.ReceiveTimeout = 10;

        ConnectionSettings = connectionSettings ?? new ConnectionSettings();
        MaxConnections = maxConnections;
        MaxConnectionsPerIp = maxConnectionsPerIp;

        Logger.LogDebug("The network server has been successfully initialized");
        Logger.LogInformation("Local address: {EndPoint}", EndPoint.ToString());

        MapConnections = new Dictionary<int, int>(MaxConnections);

        Connections = Enumerable.Range(0, MaxConnections)
           .Select(CreateEmptyConnection)
           .ToArray();

        RefreshSecurityTokenSeed();

        return true;
    }

    protected virtual INetworkConnection CreateEmptyConnection(int id)
    {
        return new NetworkConnection(id, Socket, ConnectionSettings);
    }

    public bool TryGetConnectionId(IPEndPoint endPoint, out int id)
    {
        return MapConnections.TryGetValue(endPoint.GetHashCode(), out id);
    }

    public IEnumerable<NetworkMessage> GetMessages(CancellationToken cancellationToken)
    {
        var endPoint = default(IPEndPoint);

        while (!cancellationToken.IsCancellationRequested && Socket.Available > 0)
        {
            Span<byte> data;

            try
            {
                data = Socket.Receive(ref endPoint).AsSpan();
            }
            catch (SocketException e)
            {
                continue;
            }

            if (!PacketUnpacker.TryUnpack(data, out var packet))
                continue;

            if (packet.Flags.HasFlag(NetworkPacketFlags.ConnectionLess))
            {
                yield return new NetworkMessage(
                    connectionId: -1,
                    endPoint: endPoint,
                    data: packet.Data,
                    extraData: packet.ExtraData
                );

                continue;
            }

            if (packet.Data.Length == 0 &&
                packet.Flags.HasFlag(NetworkPacketFlags.Connection))
            {
                continue;
            }

            if (TryGetConnectionId(endPoint, out var connectionId))
            {
                // TODO
                // if (!packet.IsSixup &&
                //     Connections[connectionId].IsSixup != null &&
                //     Connections[connectionId].IsSixup!.Value)
                // {
                //     throw new NotImplementedException();
                // }

                if (packet.Flags.HasFlag(NetworkPacketFlags.Connection))
                {
                    if ((ConnectionStateMsg)packet.Data[0] != ConnectionStateMsg.Close)
                    {
                        // TODO reconnect
                        // throw new NotImplementedException();
                    }
                }

                foreach (var message in Connections[connectionId].ProcessPacket(endPoint, packet))
                    yield return message;
            }
            else
            {
                ProcessConnectionStateMessage(endPoint, packet);
            }
        }
    }

    public void Update()
    {
        for (var i = 0; i < Connections.Count; i++)
        {
            Connections[i].Update();

            if (Connections[i].State is ConnectionState.Timeout or ConnectionState.Disconnecting)
            {
                Drop(i, Connections[i].State switch
                {
                    ConnectionState.Timeout => "Timeout",
                    _ => string.Empty,
                });
            }
        }
    }

    public void SendData(
        IPEndPoint endPoint,
        ReadOnlySpan<byte> data,
        ReadOnlySpan<byte> extraData = default)
    {
        NetworkHelper.SendData(Socket, endPoint, data, extraData);
    }

    public void Send(
        int connectionId,
        Span<byte> data,
        NetworkSendFlags sendFlags)
    {
        if (data.Length >= NetworkConstants.MaxPayload)
        {
            Logger.LogDebug("Dropping packet, packet payload too big ({Length})", data.Length);
            return;
        }

        var flags = NetworkMessageHeaderFlags.None;

        if (sendFlags.HasFlag(NetworkSendFlags.Vital))
            flags |= NetworkMessageHeaderFlags.Vital;

        if (!Connections[connectionId].QueueMessage(data, flags))
            return;

        if (sendFlags.HasFlag(NetworkSendFlags.Flush))
            Connections[connectionId].FlushMessages();
    }

    public void Drop(int connectionId, string reason)
    {
        var connection = Connections[connectionId];
        Logger.LogDebug("Drop connection, reason: '{Reason}' ({EndPoint})",
            reason, connection.EndPoint.ToString());

        MapConnections.Remove(connection.EndPoint.GetHashCode());
        connection.Disconnect(reason);
        ConnectionDropped?.Invoke(connection, reason);
    }

    protected virtual void ProcessConnectionStateMessage(
        IPEndPoint endPoint,
        NetworkPacketIn packetIn)
    {
        if (packetIn.Data.Length == 0 || !packetIn.Flags.HasFlag(NetworkPacketFlags.Connection))
            return;

        var msg = (ConnectionStateMsg)packetIn.Data[0];
        Logger.LogDebug("ProcessConnectionStateMessage: {Msg} from {EndPoint}", msg, endPoint.ToString());

        switch (msg)
        {
            case ConnectionStateMsg.Connect:
                if (packetIn.Data.Length >= 1 + StructHelper<SecurityToken>.Size * 2
                    && packetIn.Data.AsSpan(1, StructHelper<SecurityToken>.Size) == SecurityToken.Magic)
                {
                    OnConnectionStateConnectMsg(endPoint, packetIn);
                }

                break;

            case ConnectionStateMsg.Accept:
                if (packetIn.Data.Length >= 1 + StructHelper<SecurityToken>.Size)
                {
                    OnConnectionStateAcceptMsg(endPoint, packetIn);
                }

                break;
        }
    }

    protected virtual void OnConnectionStateConnectMsg(
        IPEndPoint endPoint,
        NetworkPacketIn packetIn)
    {
        SendConnectionStateMsg(
            endPoint: endPoint,
            msg: ConnectionStateMsg.ConnectAccept,
            token: GetToken(endPoint),
            extraData: SecurityToken.Magic
        );
    }

    protected virtual void OnConnectionStateAcceptMsg(
        IPEndPoint endPoint,
        NetworkPacketIn packetIn)
    {
        var token = (SecurityToken) packetIn.Data.AsSpan(1);

        if (token == GetToken(endPoint))
            TryAcceptConnection(endPoint, token);
        else
            Logger.LogDebug("Invalid token ({EndPoint})", endPoint.ToString());
    }

    protected virtual bool TryAcceptConnection(
        IPEndPoint endPoint,
        SecurityToken token)
    {
        if (token == SecurityToken.Unknown ||
            token == SecurityToken.Unsupported)
        {
            OnRejectConnectionUnsupportedToken(endPoint, token);
            return false;
        }

        if (GetConnectionsCountWithSameAddress(endPoint, out var emptyConnectionId) + 1 > MaxConnectionsPerIp)
        {
            OnRejectConnectionToManySameIP(endPoint, token);
            return false;
        }

        if (emptyConnectionId == -1)
        {
            OnRejectConnectionServerIsFull(endPoint, token);
            return false;
        }

        Connections[emptyConnectionId].Init(endPoint, token);
        MapConnections.Add(endPoint.GetHashCode(), emptyConnectionId);

        Logger.LogDebug("Connection accepted ({EndPoint})", endPoint.ToString());
        ConnectionAccepted(Connections[emptyConnectionId]);

        return true;
    }

    protected virtual void OnRejectConnectionUnsupportedToken(
        IPEndPoint endPoint,
        SecurityToken token)
    {
        SendConnectionStateMsg(
            endPoint: endPoint,
            msg: ConnectionStateMsg.Close,
            token: token,
            extraMsg: "Unsupported or Unknown security token"
        );
    }

    protected virtual void OnRejectConnectionToManySameIP(
        IPEndPoint endPoint,
        SecurityToken token)
    {
        SendConnectionStateMsg(
            endPoint: endPoint,
            msg: ConnectionStateMsg.Close,
            token: token,
            extraMsg: $"Only {MaxConnectionsPerIp} players with the same IP are allowed"
        );
    }

    protected virtual void OnRejectConnectionServerIsFull(
        IPEndPoint endPoint,
        SecurityToken token)
    {
        SendConnectionStateMsg(
            endPoint: endPoint,
            msg: ConnectionStateMsg.Close,
            token: token,
            extraMsg: "This server is full"
        );
    }

    protected virtual int GetConnectionsCountWithSameAddress(IPEndPoint endPoint, out int emptyConnectionId)
    {
        var count = 0;
        emptyConnectionId = -1;

        for (var id = Connections.Count - 1; id >= 0; id--)
        {
            switch (Connections[id].State)
            {
                case ConnectionState.Offline:
                    emptyConnectionId = id;
                    continue;
                case ConnectionState.Timeout:
                    continue;
            }

            if (Connections[id].EndPoint.Address.Equals(endPoint.Address))
                count++;
        }

        return count;
    }

    protected virtual void SendConnectionStateMsg(
        IPEndPoint endPoint,
        ConnectionStateMsg msg,
        SecurityToken token,
        string? extraMsg = null)
    {
        NetworkHelper.SendConnectionStateMsg(
            Socket,
            endPoint,
            msg,
            token,
            0,
            extraMsg
        );
    }

    protected virtual void SendConnectionStateMsg(
        IPEndPoint endPoint,
        ConnectionStateMsg msg,
        SecurityToken token,
        byte[] extraData)
    {
        Logger.LogDebug("SendConnectionStateMsg: {Msg} to {EndPoint}", msg, endPoint.ToString());
        NetworkHelper.SendConnectionStateMsg(
            Socket,
            endPoint,
            msg,
            token,
            0,
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
        if (Socket != null!)
        {
            Socket.Close();
            Socket.Dispose();
            Socket = null!;
        }
    }
}

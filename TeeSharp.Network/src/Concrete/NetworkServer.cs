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
using Microsoft.Extensions.Logging.Abstractions;
using TeeSharp.Core.Helpers;
using TeeSharp.Network.Abstract;

namespace TeeSharp.Network.Concrete;

[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
public class NetworkServer : INetworkServer
{
    public event Action<INetworkConnection> ConnectionAccepted = delegate {  };
    public event Action<INetworkConnection, string>? ConnectionDropped;

    public NetworkServerConfig Config { get; }
    public INetworkPacketUnpacker PacketUnpacker { get; protected set; }
    public IReadOnlyList<INetworkConnection> Connections { get; }

    protected ILogger Logger { get; set; }
    protected Dictionary<int, int> ConnectionsMap { get; }
    protected EndPoint EndPoint => Socket.Client.LocalEndPoint!;
    protected UdpClient Socket { get; set; } = null!;
    protected byte[] SecurityTokenSeed { get; set; } = null!;

    private bool _disposedValue;

    public NetworkServer(NetworkServerConfig config, ILoggerFactory? loggerFactory = default)
    {
        Config = config;
        Logger = loggerFactory?.CreateLogger("NetworkServer") ?? NullLogger.Instance;
        PacketUnpacker = CreatePacketUnpacker();
        ConnectionsMap = new Dictionary<int, int>(Config.MaxConnections);
        Connections = Enumerable.Range(0, Config.MaxConnections)
            .Select(CreateEmptyConnection)
            .ToArray();
    }

    protected virtual INetworkPacketUnpacker CreatePacketUnpacker()
    {
        return new NetworkPacketUnpacker();
    }

    protected virtual INetworkConnection CreateEmptyConnection(int connectionId)
    {
        return new NetworkConnection(connectionId, Config.Connection, Logger);
    }

    public static IPEndPoint GetBindAddress(int port, string bindAddress)
    {
        var ip = string.IsNullOrEmpty(bindAddress)
            ? IPAddress.Any
            : IPAddress.Parse(bindAddress);

        return new IPEndPoint(ip, port);
    }

    public virtual void Init()
    {
        var localEP = GetBindAddress(Config.Port, Config.BindAddress);

        try
        {
            Socket = new UdpClient(localEP);
            Socket.Client.Blocking = true;
            Socket.Client.ReceiveTimeout = 10;
        }
        catch (Exception e)
        {
            if (e is SocketException { SocketErrorCode: SocketError.AddressAlreadyInUse })
                Logger.LogError("Couldn't open socket, port {Port} already be in use", Config.Port);
            else
                Logger.LogError(exception: e, message: "Couldn't open socket");

            throw;
        }

        SetConnectionsSocket();
        RefreshSecurityTokenSeed();

        Logger.LogInformation("Network server initialized, local address: {EndPoint}", EndPoint.ToString());
    }

    protected virtual void SetConnectionsSocket()
    {
        foreach (var connection in Connections)
            connection.SetSocket(Socket);
    }

    public virtual bool TryGetLocalEndPoint([NotNullWhen(true)] out EndPoint? localEndPoint)
    {
        if (Socket == null!)
        {
            localEndPoint = null;
            return false;
        }

        localEndPoint = Socket.Client.LocalEndPoint!;
        return true;
    }

    public virtual bool TryGetConnectionId(IPEndPoint endPoint, out int id)
    {
        return ConnectionsMap.TryGetValue(endPoint.GetHashCode(), out id);
    }

    public virtual IEnumerable<NetworkMessage> GetMessages(CancellationToken cancellationToken)
    {
        var endPoint = default(IPEndPoint);

        while (!cancellationToken.IsCancellationRequested && Socket.Available > 0)
        {
            Span<byte> data;

            try
            {
                data = Socket.Receive(ref endPoint).AsSpan();
            }
            catch (SocketException)
            {
                continue;
            }

            if (PacketUnpacker.TryUnpack(data, out var packet) == false)
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

    public virtual void Update()
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

    public virtual void SendData(
        IPEndPoint endPoint,
        ReadOnlySpan<byte> data,
        ReadOnlySpan<byte> extraData = default)
    {
        NetworkHelper.SendData(Socket, endPoint, data, extraData);
    }

    public virtual void Send(
        int connectionId,
        Span<byte> data,
        NetworkSendFlags sendFlags)
    {
        if (data.Length >= NetworkConstants.MaxPayload)
        {
            Logger.LogDebug("Dropping packet, packet payload too big ({Length})", data.Length);
            return;
        }

        if (sendFlags.HasFlag(NetworkSendFlags.ConnectionLess))
        {
            // throw new NotImplementedException();
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

    public virtual void Drop(int connectionId, string reason)
    {
        var connection = Connections[connectionId];
        Logger.LogDebug("Drop connection, reason: '{Reason}' ({EndPoint})",
            reason, connection.EndPoint.ToString());

        ConnectionsMap.Remove(connection.EndPoint.GetHashCode());
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
                else
                {
                    // TODO (?)

                    SendConnectionStateMsg(
                        endPoint: endPoint,
                        msg: ConnectionStateMsg.ConnectAccept,
                        token: SecurityToken.Unsupported,
                        extraData: Array.Empty<byte>()
                    );

                    SendConnectionStateMsg(
                        endPoint: endPoint,
                        msg: ConnectionStateMsg.Close,
                        token: SecurityToken.Unsupported,
                        extraMsg: "Download the latest DDNet client from https://ddnet.org/"
                    );
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

        if (GetConnectionsCountWithSameAddress(endPoint, out var emptyConnectionId) + 1 > Config.MaxConnectionsPerIp)
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
        ConnectionsMap.Add(endPoint.GetHashCode(), emptyConnectionId);

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
            extraMsg: $"Only {Config.MaxConnectionsPerIp} players with the same IP are allowed"
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
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposedValue)
            return;

        if (disposing)
        {
            if (Socket != null!)
            {
                Socket.Close();
                Socket.Dispose();
                Socket = null!;
            }
        }

        _disposedValue = true;
    }
}

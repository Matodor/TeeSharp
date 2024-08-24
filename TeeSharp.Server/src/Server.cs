using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TeeSharp.Common;
using TeeSharp.Common.Extensions;
using TeeSharp.Core;
using TeeSharp.Core.Helpers;
using TeeSharp.Network;
using TeeSharp.Network.Abstract;
using TeeSharp.Network.Concrete;
using Uuids;

namespace TeeSharp.Server;

public class Server
{
    public event Action<Server, INetworkConnection> OnConnectionAccept = delegate {};
    public event Action<Server, INetworkConnection> OnConnectionDrop = delegate {};

    /// <summary>
    /// Current tick
    /// </summary>
    public int Tick { get; private set; }

    /// <summary>
    /// Ticks per second
    /// </summary>
    public int TickRate { get; }

    /// <summary>
    /// TODO
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// TODO
    /// </summary>
    public ClientsContainer Clients { get; }

    /// <summary>
    /// TODO
    /// </summary>
    public int ClientsCount => Clients.All.Count;

    /// <summary>
    /// TODO
    /// </summary>
    public ServerConfig Config { get; }

    protected INetworkServer NetworkServer { get; }
    protected ILogger Logger { get; set; }
    protected ILoggerFactory LoggerFactory { get; set; }

    protected IDictionary<Uuid, MessageCallback> ClientUuidMessageHandlers { get; }
    protected IDictionary<Protocol.Message, MessageCallback> ClientMessageHandlers { get; }

    protected delegate void MessageCallback(int connectionId, Unpacker unpacker, IPEndPoint endPoint);

    public Server(
        ServerConfig config,
        ILoggerFactory? loggerFactory = default)
    {
        Config = config;
        LoggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        Logger = LoggerFactory.CreateLogger("Server");

        Tick = 0;
        TickRate = config.TickRate;

        NetworkServer = CreateNetworkServer();
        NetworkServer.ConnectionAccepted += NetworkServerOnConnectionAccepted;
        NetworkServer.ConnectionDropped += NetworkServerOnConnectionDropped;

        Clients = CreateClientsContainer();
        ClientUuidMessageHandlers = new Dictionary<Uuid, MessageCallback>();
        ClientMessageHandlers = new Dictionary<Protocol.Message, MessageCallback>();

        SetClientUuidMessageHandlers();
        SetClientMessageHandlers();
    }

    protected virtual ClientsContainer CreateClientsContainer()
    {
        return new ClientsContainer(NetworkServer.Connections.Count);
    }

    protected virtual INetworkServer CreateNetworkServer()
    {
        return new NetworkServer(Config.Network, LoggerFactory);
    }

    protected virtual void NetworkServerOnConnectionAccepted(
        INetworkConnection connection)
    {
        Clients.GetByConnectionId(connection.Id)
            .Reset()
            .SetState(ClientState.PreAuth);

        OnConnectionAccept(this, connection);
    }

    protected virtual void NetworkServerOnConnectionDropped(
        INetworkConnection connection,
        string reason)
    {
        var client = Clients.GetByConnectionId(connection.Id);

        OnConnectionDrop(this, connection);
    }

    protected virtual void SetClientUuidMessageHandlers()
    {
        ClientUuidMessageHandlers[Protocol.MessageExtended.DDNet.ClientVersion] = OnClientMessageDDNetVersion;
        ClientUuidMessageHandlers[Protocol.MessageExtended.DDNet.Ping] = OnClientMessageDDNetPing;
    }

    protected virtual void SetClientMessageHandlers()
    {
        ClientMessageHandlers[Protocol.Message.ClientInfo] = OnClientMessageInfo;
        ClientMessageHandlers[Protocol.Message.ClientRequestMapData] = OnClientMessageRequestMapData;
        ClientMessageHandlers[Protocol.Message.ClientReady] = OnClientMessageReady;
        ClientMessageHandlers[Protocol.Message.ClientEnterGame] = OnClientMessageEnterGame;
        ClientMessageHandlers[Protocol.Message.ClientInput] = OnClientMessageInput;
        ClientMessageHandlers[Protocol.Message.ClientRconCommand] = OnClientMessageRconCommand;
        ClientMessageHandlers[Protocol.Message.ClientRconAuth] = OnClientMessageRconAuth;
        ClientMessageHandlers[Protocol.Message.Ping] = OnClientMessagePing;
    }

    public virtual async Task StopAsync()
    {
        IsRunning = false;
    }

    public virtual async Task RunAsync(
        CancellationToken cancellationToken = default)
    {
        if (IsRunning)
            return;

        if (InitNetworkServer() == false)
            return;

        IsRunning = true;

        const long ticksPerMillisecond = 10000;
        const long ticksPerSecond = ticksPerMillisecond * 1000;
        const long maxElapsedTicks = ticksPerMillisecond * 500;

        var targetElapsedTicks = ticksPerSecond / TickRate;
        var accumulatedElapsedTicks = 0L;
        var gameTimer = Stopwatch.StartNew();
        var prevElapsedTicks = 0L;

        while (IsRunning)
        {
            BeginLoop:

            var currentTicks = gameTimer.ElapsedTicks;
            accumulatedElapsedTicks += currentTicks - prevElapsedTicks;
            prevElapsedTicks = currentTicks;

            if (accumulatedElapsedTicks < targetElapsedTicks)
            {
                var sleepTime = (targetElapsedTicks - accumulatedElapsedTicks) / (double) ticksPerMillisecond;
#if _WINDOWS
                ThreadsHelper.SleepForNoMoreThan(sleepTime);
#else
                if (sleepTime >= 2)
                    Thread.Sleep(1);
#endif
                goto BeginLoop;
            }

            while (accumulatedElapsedTicks >= targetElapsedTicks)
            {
                accumulatedElapsedTicks -= targetElapsedTicks;

                BeforeUpdate();
                Update();
                LateUpdate();
            }

            UpdateSnapshots();
            UpdateNetwork(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                Logger.LogInformation("Execution interrupted");
                IsRunning = false;

                break;
            }
        }

        Logger.LogInformation("Server shutdown");
        IsRunning = false;
    }

    protected virtual bool InitNetworkServer()
    {
        try
        {
            NetworkServer.Init();
        }
        catch (Exception)
        {
            Logger.LogError("Couldn't initialize network server");
            return false;
        }

        return true;
    }

    protected virtual void BeforeUpdate()
    {

    }

    protected virtual void Update()
    {
        ProcessClientsPredictedEarlyInput();
        ++Tick;
        ProcessClientsPredictedInput();

        // Game Tick
    }

    protected virtual void LateUpdate()
    {

    }

    protected virtual void ProcessClientsPredictedEarlyInput()
    {

    }

    protected virtual void ProcessClientsPredictedInput()
    {
    }

    protected virtual void UpdateSnapshots()
    {
        if (Config.HighBandwidth == false && Tick % 2 != 0)
            return;

        // TODO
    }

    protected virtual void UpdateNetwork(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return;

        NetworkServer.Update();

        foreach (var message in NetworkServer.GetMessages(cancellationToken))
        {
            ProcessNetworkMessage(message);
        }
    }

    protected virtual void ProcessNetworkMessage(NetworkMessage message)
    {
        if (message.ConnectionId == -1)
            ProcessMasterServerMessage(message);
        else
            ProcessClientMessage(message);
    }

    protected virtual void ProcessMasterServerMessage(NetworkMessage message)
    {
        // TODO

        // if (_interactor.ProcessMasterServerPacket(message.Data, message.EndPoint))
        //     return;
        //
        // if (message.ExtraData.Length > 0 &&
        //     MasterServerPackets.GetInfo.Length + 1 <= message.Data.Length &&
        //     MasterServerPackets.GetInfo.AsSpan()
        //         .SequenceEqual(message.Data.AsSpan(0, MasterServerPackets.GetInfo.Length)))
        // {
        //     var extraToken = ((message.ExtraData[0] << 8) | message.ExtraData[1]) << 8;
        //     var token = (SecurityToken) (message.Data[MasterServerPackets.GetInfo.Length] | extraToken);
        //
        //     SendServerInfoConnectionLess(message.EndPoint, token);
        // }
    }

    protected virtual void ProcessClientMessage(NetworkMessage message)
    {
        var unPacker = new Unpacker(message.Data);
        if (unPacker.TryGetMessageInfo(out var msgId, out var msgUuid, out var isSystemMsg))
        {
            if (isSystemMsg == false)
            {
                throw new NotImplementedException();

                // if (_clients[message.ConnectionId].ClientState >= ClientState.Ready)
                // {
                //     GameContext.Instance.OnMessage(
                //         (GameMessage)msgId,
                //         msgUuid,
                //         unPacker,
                //         message.ConnectionId
                //     );
                // }
                // return;
            }

            if (msgId == Protocol.Message.Empty)
            {
                ProcessClientSystemUuidMessage(
                    message.ConnectionId,
                    msgUuid,
                    unPacker,
                    message.EndPoint
                );
            }
            else
            {
                ProcessClientSystemMessage(
                    message.ConnectionId,
                    msgId,
                    unPacker,
                    message.EndPoint
                );
            }
        }
        else
        {
            ProcessUnknownClientMessage(
                message.ConnectionId,
                msgId,
                msgUuid,
                unPacker,
                message.EndPoint
            );
        }
    }

    protected virtual void ProcessClientSystemUuidMessage(
        int connectionId,
        Uuid msgUuid,
        Unpacker unpacker,
        IPEndPoint endPoint)
    {
        if (ClientUuidMessageHandlers.TryGetValue(msgUuid, out var callback))
        {
            callback(connectionId, unpacker, endPoint);
        }
        else
        {
            Logger.LogDebug("Unknown client system uuid message: {Uuid}", msgUuid.ToString("D"));
        }
    }

    protected virtual void ProcessClientSystemMessage(
        int connectionId,
        Protocol.Message msgId,
        Unpacker unpacker,
        IPEndPoint endPoint)
    {
        if (ClientMessageHandlers.TryGetValue(msgId, out var callback))
        {
            callback(connectionId, unpacker, endPoint);
        }
        else
        {
            Logger.LogDebug("Unknown client system message: {Uuid} from {EndPoint}", msgId, endPoint.ToString());
        }
    }

    protected virtual void ProcessUnknownClientMessage(
        int connectionId,
        Protocol.Message msgId,
        Uuid msgUuid,
        Unpacker unpacker,
        IPEndPoint endPoint)
    {
        Logger.LogDebug("Unknown client message, ConnectionId({ConnectionId}) MsgId({MsgId}) MsgUUID({UUID}) Length({Length})", connectionId, msgId, msgUuid, unpacker.DataOriginal.Length);
    }

    protected virtual void OnClientMessageDDNetVersion(int connectionId, Unpacker unpacker, IPEndPoint endpoint)
    {
        Logger.LogDebug("OnClientMessageDDNetVersion");

        var client = Clients.GetByConnectionId(connectionId);
        if (client.State != ClientState.PreAuth)
            return;

        if (!unpacker.TryGetUuid(out var connectionUuid) ||
            !unpacker.TryGetInteger(out var versionNum) ||
            !unpacker.TryGetString(out var version))
        {
            return;
        }

        if (versionNum < 0 || string.IsNullOrWhiteSpace(version))
            return;

        client
            .SetConnectionUuid(connectionUuid)
            .SetDDNetVersion(version, versionNum)
            .SetState(ClientState.Auth);
    }

    protected virtual void OnClientMessageInfo(int connectionId, Unpacker unpacker, IPEndPoint endpoint)
    {
        var client = Clients.GetByConnectionId(connectionId);
        if (client.State is not (ClientState.PreAuth or ClientState.Auth))
            return;

        if (!unpacker.TryGetString(out var version))
            return;

        client.SetVersion(version);

        if (!CheckClientVersion(client, out var reason))
        {
            NetworkServer.Drop(connectionId, reason);
            return;
        }

        if (!unpacker.TryGetString(out var password))
            return;

        if (!string.IsNullOrEmpty(Config.Password) && Config.Password != password)
        {
            NetworkServer.Drop(connectionId, "Wrong password");
            return;
        }

        if (client.Id >= ClientsCount - Config.ReservedSlots)
        {
            if (CheckReservedSlotPassword(client, password))
            {
                throw new NotImplementedException();
            }
            else
            {
                NetworkServer.Drop(connectionId, "This server is full");
                return;
            }
        }

        client.SetState(ClientState.Connecting);

        // TODO
        // SendRconType(client, requireUsername);
        SendSupportedCapabilities(client);
        SendMap(client);
    }

    protected virtual void OnClientMessageDDNetPing(int connectionId, Unpacker unpacker, IPEndPoint endpoint)
    {
        throw new NotImplementedException();
    }

    protected virtual void OnClientMessageRequestMapData(int connectionId, Unpacker unpacker, IPEndPoint endpoint)
    {
        throw new NotImplementedException();
    }

    protected virtual void OnClientMessageReady(int connectionId, Unpacker unpacker, IPEndPoint endpoint)
    {
        throw new NotImplementedException();
    }

    protected virtual void OnClientMessageEnterGame(int connectionId, Unpacker unpacker, IPEndPoint endpoint)
    {
        throw new NotImplementedException();
    }

    protected virtual void OnClientMessageInput(int connectionId, Unpacker unpacker, IPEndPoint endpoint)
    {
        throw new NotImplementedException();
    }

    protected virtual void OnClientMessageRconCommand(int connectionId, Unpacker unpacker, IPEndPoint endpoint)
    {
        throw new NotImplementedException();
    }

    protected virtual void OnClientMessageRconAuth(int connectionId, Unpacker unpacker, IPEndPoint endpoint)
    {
        throw new NotImplementedException();
    }

    protected virtual void OnClientMessagePing(int connectionId, Unpacker unpacker, IPEndPoint endpoint)
    {
        throw new NotImplementedException();
    }

    protected virtual Protocol.Capabilities GetSupportedCapabilities()
    {
        var capabilities =
            Protocol.Capabilities.ChatTimeoutCode |
            Protocol.Capabilities.AnyPlayerFlag |
            Protocol.Capabilities.PingExtended |
            Protocol.Capabilities.SyncWeaponInput |
            Protocol.Capabilities.DDNet;

        if (Config.AllowDummy)
            capabilities |= Protocol.Capabilities.AllowDummy;

        return capabilities;
    }

    protected virtual bool CheckClientVersion(Client client, [NotNullWhen(false)] out string? reason)
    {
        // TODO
        //
        // if (_clients[clientId].ClientState != ClientState.GotClientVersion ||
        //     _clients[clientId].ClientVersion < 18000)
        // {
        //     NetworkServer.Drop(clientId, "Download the latest DDNet client from https://ddnet.org/");
        //     return;
        // }

        reason = null;
        return true;
    }

    protected virtual bool CheckReservedSlotPassword(Client client, string password)
    {
        throw new NotImplementedException();
    }

    protected virtual void SendRconType(Client client, bool requireUsername)
    {
        var packer = new Packer();
        packer.AddProtocolMessageExtended(Protocol.MessageExtended.DDNet.RconType, true);
        packer.AddBoolean(requireUsername);

        SendMessage(client, packer, NetworkSendFlags.Vital);
    }

    protected virtual void SendSupportedCapabilities(Client client)
    {
        var packer = new Packer();
        packer.AddProtocolMessageExtended(Protocol.MessageExtended.DDNet.Capabilities, true);
        packer.AddEnum(Protocol.Capabilities.CurrentVersion);
        packer.AddEnum(GetSupportedCapabilities());

        SendMessage(client, packer, NetworkSendFlags.Vital);
    }

    protected virtual void SendMap(Client client)
    {
        // // UuidManager.DDNet.Capabilities
        // {
        //     cl_map_download_url
        //
        //     var mapUrl = $"https://raw.githubusercontent.com/tee-community/FlatCity-maps/main/{GetMapName()}.map";
        //     var packer = new Packer(UuidManager.DDNet.MapDetails, true);
        //
        //     packer.AddString(GetMapName());
        //     packer.AddRaw(Map.Sha256.Span);
        //     packer.AddInteger((int)Map.Checksum);
        //     packer.AddInteger(Map.Size);
        //     packer.AddString(mapUrl);
        //
        //     SendMessage(clientId, packer, NetworkSendFlags.Vital);
        // }

        // ProtocolMessage.ServerMapChange
        {
            var packer = new Packer();
            packer.AddProtocolMessage(Protocol.Message.ServerMapChange);
            // packer.AddString(GetMapName());
            // packer.AddInteger((int)Map.Checksum);
            // packer.AddInteger(Map.Size);

            packer.AddString("dm1");
            packer.AddInteger(unchecked((int)4061503086));
            packer.AddInteger(5805);

            SendMessage(client, packer, NetworkSendFlags.Vital | NetworkSendFlags.Flush);
        }
    }

    public void SendMessage(Client? client, Packer packer, NetworkSendFlags flags)
    {
        if (packer.HasError)
            return;

        if (client == null)
        {
            throw new NotImplementedException();

            // for (var i = 0; i < _clients.Length; i++)
            // {
            //     if (_clients[i].ClientState != ClientState.InGame)
            //         continue;
            //
            //     NetworkServer.Send(i, packer.Buffer, flags);
            // }
        }
        else
        {
            NetworkServer.Send(client.Id, packer.Buffer, flags);
        }
    }
}


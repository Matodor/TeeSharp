using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.Extensions.Logging;
using TeeSharp.Common.Protocol;
using TeeSharp.Core;
using TeeSharp.Core.Helpers;
using TeeSharp.Common.Settings;
using TeeSharp.Core.Extensions;
using TeeSharp.MasterServer;
using TeeSharp.Network;
using TeeSharp.Network.Abstract;
using TeeSharp.Network.Concrete;
using TeeSharp.Server.Abstract;
using Uuids;

namespace TeeSharp.Server.Concrete;

public class Server : IServer
{
    public const long TicksPerMillisecond = 10000;
    public const long TicksPerSecond = TicksPerMillisecond * 1000;

    public int TickRate { get; }
    public int Tick { get; private set; }
    public TimeSpan GameTime { get; private set; }
    public ServerState ServerState { get; private set; }
    public ServerSettings Settings { get; private set; }
    public IReadOnlyList<IServerClient> Clients { get; private set; }

    protected ILogger Logger { get; set; }
    protected INetworkServer NetworkServer { get; set; }
    protected long PrevTicks { get; set; }
    protected CancellationTokenSource? CtsServer { get; private set; }

    protected TimeSpan TargetElapsedTime { get; }
    protected TimeSpan MaxElapsedTime { get; }

    protected Dictionary<Uuid, MessageCallback> ClientUuidMessageHandlers { get; set; }
    protected Dictionary<ProtocolMessage, MessageCallback> ClientMessageHandlers { get; set; }

    protected delegate void MessageCallback(
        int connectionId,
        UnPacker unPacker,
        IPEndPoint endPoint,
        NetworkMessageFlags flags
    );

    private readonly IDisposable? _settingsChangesListener;

    public Server(
        ISettingsChangesNotifier<ServerSettings> serverSettingsNotifier,
        ILogger? logger = null,
        int tickRate = 50)
    {
        _settingsChangesListener = serverSettingsNotifier.Subscribe(OnChangeSettings);

        TickRate = tickRate;
        TargetElapsedTime = TimeSpan.FromTicks(TicksPerSecond / TickRate);
        MaxElapsedTime = TimeSpan.FromMilliseconds(500);

        ClientUuidMessageHandlers = new Dictionary<Uuid, MessageCallback>();
        ClientMessageHandlers = new Dictionary<ProtocolMessage, MessageCallback>();
        SetClientUuidMessageHandlers();
        SetClientMessageHandlers();

        Logger = logger ?? Tee.LoggerFactory.CreateLogger("GameServer");
        Settings = serverSettingsNotifier.Current;
        ServerState = ServerState.StartsUp;
        NetworkServer = CreateNetworkServer();
        NetworkServer.ConnectionAccepted += NetworkServerOnConnectionAccepted;

        Clients = Enumerable.Range(0, Settings.MaxConnections)
            .Select(CreateClient)
            .ToArray();
    }

    protected virtual IServerClient CreateClient(int id)
    {
        return new ServerClient(id);
    }

    protected virtual void NetworkServerOnConnectionAccepted(INetworkConnection connection)
    {
        Clients[connection.Id].State = ServerClientState.PreAuth;
    }

    protected virtual void SetClientUuidMessageHandlers()
    {
        SetClientUuidMessageHandler(UuidManager.DDNet.ClientVersion, OnUuidDDNetClientVersionMessage);
    }

    protected virtual void SetClientMessageHandlers()
    {
        SetClientMessageHandler(ProtocolMessage.ClientInfo, OnClientInfoMessage);
    }

    protected virtual INetworkServer CreateNetworkServer()
    {
        return new NetworkServer();
    }

    protected virtual void OnChangeSettings(ServerSettings changedSettings)
    {
        if (Settings.UseHotReload)
        {
            // TODO
            Logger.LogInformation("The settings changes have been applied");
        }
        else
        {
            Logger.LogInformation("Hot reload disabled, changes ignored");
        }
    }

    protected virtual void BeforeRun()
    {
    }

    protected virtual IPEndPoint GetNetworkServerBindAddress()
    {
        return string.IsNullOrEmpty(Settings.BindAddress)
            ? new IPEndPoint(IPAddress.Any, Settings.Port)
            : new IPEndPoint(IPAddress.Parse(Settings.BindAddress), Settings.Port);
    }

    protected virtual bool TryInitNetworkServer()
    {
        return NetworkServer.TryInit(
            localEP: GetNetworkServerBindAddress(),
            maxConnections: Settings.MaxConnections,
            maxConnectionsPerIp: Settings.MaxConnectionsPerIp
        );
    }

    public void Run(CancellationToken cancellationToken)
    {
        if (ServerState is ServerState.Running or ServerState.Stopping)
        {
            Logger.LogWarning("Unable to start the server");
            Logger.LogWarning("Current server state: {ServerState}", ServerState);
            return;
        }

        CtsServer = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        if (!TryInitNetworkServer())
        {
            Logger.LogError("Error during network server initialization");
            return;
        }

        Logger.LogInformation("Server name: {ServerName}", Settings.Name);
        ServerState = ServerState.Running;
        Tick = 0;

        RunMainLoop();
        Stop();

        ServerState = ServerState.Stopped;
        Logger.LogInformation("Stopped");
    }

    protected virtual void BeforeStop()
    {
    }

    public void Stop()
    {
        if (ServerState is ServerState.Stopping or ServerState.Stopped)
            return;

        BeforeStop();
        ServerState = ServerState.Stopping;
        CtsServer!.Cancel();
    }

    protected virtual void SetClientUuidMessageHandler(
        Uuid msgUuid,
        MessageCallback callback)
    {
        ClientUuidMessageHandlers.AddOrOverride(msgUuid, callback);
    }

    protected virtual void SetClientMessageHandler(
        ProtocolMessage msgId,
        MessageCallback callback)
    {
        ClientMessageHandlers.AddOrOverride(msgId, callback);
    }

    protected virtual void UpdateNetwork()
    {
        if (CtsServer!.IsCancellationRequested)
            return;

        NetworkServer.Update();

        foreach (var message in NetworkServer.GetMessages(CtsServer.Token))
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
        if (message.ExtraData.Length > 0 &&
            MasterServerPackets.GetInfo.Length + 1 <= message.Data.Length &&
            MasterServerPackets.GetInfo.AsSpan()
                .SequenceEqual(message.Data.AsSpan(0, MasterServerPackets.GetInfo.Length)))
        {
            var extraToken = ((message.ExtraData[0] << 8) | message.ExtraData[1]) << 8;
            var token = (SecurityToken) (message.Data[MasterServerPackets.GetInfo.Length] | extraToken);

            SendServerInfoConnectionLess(message.EndPoint, token);
        }
    }

    protected virtual void SendServerInfoConnectionLess(
        IPEndPoint endPoint,
        SecurityToken token)
    {
        // TODO
        SendServerInfo(endPoint, token, true);
    }

    protected virtual void SendServerInfo(
        IPEndPoint endPoint,
        SecurityToken token,
        bool sendClients)
    {

    }

    protected virtual void ProcessClientMessage(NetworkMessage message)
    {
        var unPacker = new UnPacker(message.Data);
        if (unPacker.TryGetMessageInfo(out var msgId, out var msgUuid, out var isSystemMsg))
        {
            if (!isSystemMsg)
                return;

            if (msgId == ProtocolMessage.Empty)
            {
                ProcessClientSystemUuidMessage(
                    message.ConnectionId,
                    msgUuid,
                    unPacker,
                    message.EndPoint,
                    message.Flags
                );
            }
            else
            {
                ProcessClientSystemMessage(
                    message.ConnectionId,
                    msgId,
                    unPacker,
                    message.EndPoint,
                    message.Flags
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
                message.EndPoint,
                message.Flags
            );
        }
    }

    protected virtual void ProcessClientSystemUuidMessage(
        int connectionId,
        Uuid msgUuid,
        UnPacker unPacker,
        IPEndPoint endPoint,
        NetworkMessageFlags flags)
    {
        if (ClientUuidMessageHandlers.TryGetValue(msgUuid, out var callback))
        {
            callback(connectionId, unPacker, endPoint, flags);
        }
        else
        {
            Logger.LogDebug("Unknown client system uuid message: {Uuid}", msgUuid);
        }
    }

    protected virtual void ProcessClientSystemMessage(
        int connectionId,
        ProtocolMessage msgId,
        UnPacker unPacker,
        IPEndPoint endPoint,
        NetworkMessageFlags flags)
    {
        if (ClientMessageHandlers.TryGetValue(msgId, out var callback))
        {
            callback(connectionId, unPacker, endPoint, flags);
        }
        else
        {
            Logger.LogDebug("Unknown client system message: {Uuid}", msgId);
        }
    }

    protected virtual void ProcessUnknownClientMessage(
        int connectionId,
        ProtocolMessage msgId,
        Uuid msgUuid,
        UnPacker unPacker,
        IPEndPoint endPoint,
        NetworkMessageFlags flags)
    {
        // ignore
    }

    protected virtual void OnUuidDDNetClientVersionMessage(
        int connectionId,
        UnPacker unPacker,
        IPEndPoint endpoint,
        NetworkMessageFlags flags)
    {
        if (!flags.HasFlag(NetworkMessageFlags.Vital) || Clients[connectionId].State != ServerClientState.PreAuth)
            return;

        if (!unPacker.TryGetUuid(out var connectionUuid) ||
            !unPacker.TryGetInteger(out var version) ||
            !unPacker.TryGetString(out var versionStr))
        {
            return;
        }

        if (version < 0)
            return;

        Clients[connectionId].ConnectionUuid = connectionUuid;
        Clients[connectionId].DDNetVersion = version;
        Clients[connectionId].DDNetVersionString = versionStr;
        Clients[connectionId].State = ServerClientState.Auth;
    }

    protected virtual void OnClientInfoMessage(
            int connectionId,
            UnPacker unPacker,
            IPEndPoint endpoint,
            NetworkMessageFlags flags)
    {
        if (!flags.HasFlag(NetworkMessageFlags.Vital) ||
            Clients[connectionId].State != ServerClientState.PreAuth &&
            Clients[connectionId].State != ServerClientState.Auth)
        {
            return;
        }

        if (!unPacker.TryGetString(out var version) ||
            !unPacker.TryGetString(out var password))
        {
            return;
        }


        Clients[connectionId].State = ServerClientState.Connecting;
        SendSupportedCapabilities(connectionId);
        SendMap(connectionId);

        // TODO CHECK VERSION
        // TODO CHECK PASSWORD
        // TODO RESERVED SLOT

        throw new NotImplementedException();
    }

    protected virtual ProtocolCapabilities GetSupportedCapabilities()
    {
        return
            ProtocolCapabilities.ChatTimeoutCode |
            ProtocolCapabilities.AnyPlayerFlag |
            ProtocolCapabilities.PingExtended |
            ProtocolCapabilities.SyncWeaponInput;
    }

    protected virtual void SendSupportedCapabilities(int connectionId)
    {
        var packer = new Packer(UuidManager.DDNet.Capabilities, true);
        packer.AddInt((int)ProtocolCapabilities.CurrentVersion);
        packer.AddInt((int)GetSupportedCapabilities());

        SendMessage(connectionId, packer, NetworkMessageFlags.Vital);
    }

    protected virtual void SendMessage(
        int connectionId,
        Packer packer,
        NetworkMessageFlags flags)
    {
        if (packer.HasError)
            return;

        if (connectionId < 0)
        {
            throw new NotImplementedException();
        }
        else
        {
            NetworkServer.Send(connectionId, packer.Buffer, flags);
        }
    }

    protected virtual void RunMainLoop()
    {
        CtsServer!.Token.ThrowIfCancellationRequested();

        BeforeRun();

        var accumulatedElapsedTime = TimeSpan.Zero;
        var gameTimer = Stopwatch.StartNew();

        while (!CtsServer.Token.IsCancellationRequested)
        {
            BeginLoop:

            var currentTicks = gameTimer.Elapsed.Ticks;
            accumulatedElapsedTime += TimeSpan.FromTicks(currentTicks - PrevTicks);
            PrevTicks = currentTicks;

            if (accumulatedElapsedTime < TargetElapsedTime)
            {
                var sleepTime = (TargetElapsedTime - accumulatedElapsedTime).TotalMilliseconds;
#if _WINDOWS
                ThreadsHelper.SleepForNoMoreThan(sleepTime);
#else
                if (sleepTime >= 2)
                    Thread.Sleep(1);
#endif
                goto BeginLoop;
            }

            if (accumulatedElapsedTime > MaxElapsedTime)
                accumulatedElapsedTime = MaxElapsedTime;

            while (accumulatedElapsedTime >= TargetElapsedTime)
            {
                accumulatedElapsedTime -= TargetElapsedTime;
                GameTime += TargetElapsedTime;

                ++Tick;

                BeforeUpdate();
                Update();
                AfterUpdate();
            }

            UpdateNetwork();
        }

        gameTimer.Stop();
        Logger.LogDebug("Main loop stopped");
        Logger.LogInformation("Main loop complete after: {Elapsed}", gameTimer.Elapsed.ToString("g"));
    }

    protected virtual void BeforeUpdate()
    {
    }

    /// <summary>
    /// Game server tick
    /// </summary>
    protected virtual void Update()
    {
        if (Tick % TickRate == 0)
        {
            // Logger.LogInformation("Tick: {Tick}", Tick);
            // Logger.LogInformation("GameTime: {GameTime}", GameTime);
        }
    }

    protected virtual void AfterUpdate()
    {
    }

    protected virtual void SendMap(int connectionId)
    {
        var mapDetails = new Packer(UuidManager.DDNet.MapDetails, true);
        throw new NotImplementedException();
    }

    protected virtual void LoadMap()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        _settingsChangesListener?.Dispose();
        CtsServer?.Dispose();
        CtsServer = null;
    }
}

using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Microsoft.Extensions.Logging;
using TeeSharp.Core;
using TeeSharp.Core.Helpers;
using TeeSharp.Core.Settings;
using TeeSharp.MasterServer;
using TeeSharp.Network;
using TeeSharp.Network.Abstract;
using TeeSharp.Network.Concrete;

namespace TeeSharp.Server;

public class BasicGameServer : IGameServer
{
public const long TicksPerMillisecond = 10000;
    public const long TicksPerSecond = TicksPerMillisecond * 1000;

    public int TickRate { get; }
    public int Tick { get; private set; }
    public TimeSpan GameTime { get; private set; }
    public ServerState ServerState { get; private set; }
    public ServerSettings Settings { get; private set; }

    protected ILogger Logger { get; set; }
    protected INetworkServer NetworkServer { get; set; }
    protected long PrevTicks { get; set; }
    protected CancellationTokenSource? CtsServer { get; private set; }

    protected TimeSpan TargetElapsedTime { get; }
    protected TimeSpan MaxElapsedTime { get; }

    private readonly IDisposable? _settingsChangesListener;

    public BasicGameServer(
        ISettingsChangesNotifier<ServerSettings> serverSettingsNotifier,
        ILogger? logger = null,
        int tickRate = 50)
    {
        _settingsChangesListener = serverSettingsNotifier.Subscribe(OnChangeSettings);

        TickRate = tickRate;
        TargetElapsedTime = TimeSpan.FromTicks(TicksPerSecond / TickRate);
        MaxElapsedTime = TimeSpan.FromMilliseconds(500);

        Logger = logger ?? Tee.LoggerFactory.CreateLogger("GameServer");
        Settings = serverSettingsNotifier.Current;
        ServerState = ServerState.StartsUp;
        NetworkServer = CreateNetworkServer();
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

        RunMainLoop(CtsServer.Token);
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

    protected virtual void UpdateNetwork()
    {
        if (CtsServer!.IsCancellationRequested)
            return;

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
        if (MasterServerPackets.GetInfo.Length + 1 <= message.Data.Length &&
            MasterServerPackets.GetInfo.AsSpan()
                .SequenceEqual(message.Data.AsSpan(0, MasterServerPackets.GetInfo.Length)))
        {
            if (message.ExtraData.Length > 0)
            {
                var extraToken = ((message.ExtraData[0] << 8) | message.ExtraData[1]) << 8;
                var token = (SecurityToken) (message.Data[MasterServerPackets.GetInfo.Length] | extraToken);
                throw new NotImplementedException();
            }
            else if (Settings.UseSixup && message.ResponseToken != SecurityToken.Unknown)
            {
                throw new NotImplementedException();
            }

            return;
        }
    }

    protected virtual void ProcessClientMessage(NetworkMessage message)
    {
    }

    protected virtual void SendServerInformation()
    {

    }

    protected virtual void RunMainLoop(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        BeforeRun();

        var accumulatedElapsedTime = TimeSpan.Zero;
        var gameTimer = Stopwatch.StartNew();

        while (!cancellationToken.IsCancellationRequested)
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
            Logger.LogInformation("Tick: {Tick}", Tick);
            Logger.LogInformation("GameTime: {GameTime}", GameTime);
        }
    }

    protected virtual void AfterUpdate()
    {
    }

    public void Dispose()
    {
        _settingsChangesListener?.Dispose();
        CtsServer?.Dispose();
        CtsServer = null;
    }
}

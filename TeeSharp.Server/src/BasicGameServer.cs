using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeeSharp.Core;
using TeeSharp.Core.Helpers;
using TeeSharp.Core.Settings;
using TeeSharp.Network;

namespace TeeSharp.Server;

public class BasicGameServer : IGameServer
{
    public const int TickRate = 50;
    public const long TicksPerMillisecond = 10000;
    public const long TicksPerSecond = TicksPerMillisecond * 1000;

    public int Tick { get; protected set; }
    public TimeSpan GameTime { get; protected set; }
    public ServerState ServerState { get; protected set; }
    public ServerSettings Settings { get; protected set; }

    protected ILogger Logger { get; set; }
    protected long PrevTicks { get; set; }
    protected INetworkServer NetworkServer { get; set; }

    private readonly TimeSpan _targetElapsedTime = TimeSpan.FromTicks(TicksPerSecond / TickRate);
    private readonly TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);

    private CancellationTokenSource? _ctsServer;
    private Task? _runNetworkTask;
    private Task? _runGameLoopTask;

    public BasicGameServer(
        ISettingsChangesNotifier<ServerSettings> serverSettingsNotifier,
        ISettingsChangesNotifier<NetworkServerSettings> networkSettingsNotifier)
    {
        serverSettingsNotifier.Subscribe(OnChangeSettings);

        Logger = Tee.LoggerFactory.CreateLogger("GameServer");
        Settings = serverSettingsNotifier.Current;
        NetworkServer = CreateNetworkServer(networkSettingsNotifier);
        ServerState = ServerState.StartsUp;
    }

    protected virtual INetworkServer CreateNetworkServer(
        ISettingsChangesNotifier<NetworkServerSettings> settingsChangesNotifier)
    {
        return new NetworkServer(settingsChangesNotifier);
    }

    protected virtual void OnChangeSettings(ServerSettings changedSettings)
    {
        if (Settings.UseHotReload)
        {
            Settings.UseHotReload = changedSettings.UseHotReload;

            if (Settings.Name != changedSettings.Name)
                Settings.Name = changedSettings.Name;

            Logger.LogInformation("The settings changes have been applied");
        }
        else
        {
            Logger.LogInformation("Hot reload disabled, changes ignored");
        }
    }

    protected virtual Task BeforeRun(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        if (ServerState != ServerState.StartsUp)
        {
            Logger.LogWarning("Already in `Running` state");
            return;
        }

        _ctsServer = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        await BeforeRun(_ctsServer.Token);

        Logger.LogInformation("Server name: {ServerName}", Settings.Name);
        ServerState = ServerState.Running;
        Tick = 0;

        _runNetworkTask = Task
            .Run(() => RunNetworkServer(_ctsServer.Token), _ctsServer.Token)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Logger.LogError("Network server stopped with ERROR");
                    throw task.Exception!;
                }
            }, CancellationToken.None);

        _runGameLoopTask = Task
            .Run(() => RunGameLoop(_ctsServer.Token), _ctsServer.Token)
            .ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Logger.LogError("Game loop stopped with ERROR");
                    throw task.Exception!;
                }
            }, CancellationToken.None);

        await Task.WhenAny(
            _runNetworkTask,
            _runGameLoopTask
        );

        await StopAsync();
    }

    protected virtual Task BeforeStop()
    {
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        _ctsServer!.Cancel();

        await BeforeStop();
        await Task.WhenAll(
            _runNetworkTask!,
            _runGameLoopTask!
        );

        ServerState = ServerState.Stopped;
    }

    protected virtual async Task RunNetworkServer(CancellationToken cancellationToken)
    {
        // // TODO make bind address from config
        // // ReSharper disable InconsistentNaming
        // // var localEP = NetworkBase.TryGetLocalIP(out var localIP)
        // //     ? new IPEndPoint(localIP, 8303)
        // //     : new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8303);
        // // ReSharper restore InconsistentNaming
        //
        // // ReSharper disable once InconsistentNaming
        // var localEP = new IPEndPoint(IPAddress.Any, Config.ServerPort);
        //
        // if (NetworkServer.Open(localEP))
        // {
        // Log.Information("Local address - {Address}", NetworkServer.BindAddress.ToString());
        // }

        await NetworkServer.RunAsync(cancellationToken);
    }

    protected virtual void RunGameLoop(CancellationToken cancellationToken)
    {
        var accumulatedElapsedTime = TimeSpan.Zero;
        var gameTimer = Stopwatch.StartNew();

        while (!cancellationToken.IsCancellationRequested)
        {
            BeginLoop:

            var currentTicks = gameTimer.Elapsed.Ticks;
            accumulatedElapsedTime += TimeSpan.FromTicks(currentTicks - PrevTicks);
            PrevTicks = currentTicks;

            if (accumulatedElapsedTime < _targetElapsedTime)
            {
                var sleepTime = (_targetElapsedTime - accumulatedElapsedTime).TotalMilliseconds;
#if _WINDOWS
                ThreadsHelper.SleepForNoMoreThan(sleepTime);
#else
                if (sleepTime >= 2)
                    Thread.Sleep(1);
#endif
                goto BeginLoop;
            }

            if (accumulatedElapsedTime > _maxElapsedTime)
                accumulatedElapsedTime = _maxElapsedTime;

            while (accumulatedElapsedTime >= _targetElapsedTime)
            {
                accumulatedElapsedTime -= _targetElapsedTime;
                GameTime += _targetElapsedTime;

                ++Tick;

                BeforeUpdate();
                Update();
                AfterUpdate();
            }
        }

        gameTimer.Stop();
        Logger.LogDebug("Game loop stopped");
        Logger.LogInformation("Game loop complete after: {Elapsed}", gameTimer.Elapsed.ToString("g"));
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
            Logger.LogInformation("Name: {ServerName}", Settings.Name);
        }
    }

    protected virtual void AfterUpdate()
    {
    }

    public void Dispose()
    {
        _ctsServer?.Dispose();
        _ctsServer = null;
    }
}

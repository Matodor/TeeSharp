using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TeeSharp.Core;
using TeeSharp.Core.Helpers;
using TeeSharp.Core.Settings;
using TeeSharp.Network;
using TeeSharp.Network.Abstract;
using TeeSharp.Network.Concrete;

namespace TeeSharp.Server;

public class BasicGameServer : IGameServer
{
    public const int TickRate = 50;
    public const long TicksPerMillisecond = 10000;
    public const long TicksPerSecond = TicksPerMillisecond * 1000;

    public int Tick { get; protected set; }
    public TimeSpan GameTime { get; protected set; }
    public ServerState ServerState { get; protected set; }
    public ServerSettings Settings { get; private set; }

    protected ILogger Logger { get; set; }
    protected INetworkServer NetworkServer { get; set; }
    protected long PrevTicks { get; set; }

    private readonly TimeSpan _targetElapsedTime = TimeSpan.FromTicks(TicksPerSecond / TickRate);
    private readonly TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);

    private readonly IDisposable? _settingsChangesListener;
    private CancellationTokenSource? _ctsServer;
    private Task? _runNetworkTask;
    private Task? _runGameLoopTask;

    public BasicGameServer(
        ISettingsChangesNotifier<ServerSettings> serverSettingsNotifier,
        ILogger? logger = null)
    {
        _settingsChangesListener = serverSettingsNotifier.Subscribe(OnChangeSettings);

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

    protected virtual void BeforeRun()
    {
    }

    protected virtual bool TryInitNetworkServer()
    {
        // ReSharper disable once InconsistentNaming
        IPEndPoint localEP;

        if (string.IsNullOrEmpty(Settings.BindAddress))
        {
            // TODO: Is it really necessary?
            localEP = NetworkHelper.TryGetLocalIpAddress(out var local)
                ? new IPEndPoint(local, Settings.Port)
                : new IPEndPoint(IPAddress.Loopback, Settings.Port);
        }
        else
        {
            localEP = new IPEndPoint(IPAddress.Parse(Settings.BindAddress), Settings.Port);
        }

        return NetworkServer.TryInit(localEP);
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        if (ServerState != ServerState.StartsUp)
        {
            Logger.LogWarning("Already in `Running` state");
            return;
        }

        _ctsServer = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        BeforeRun();

        if (!TryInitNetworkServer())
        {
            await StopAsync();
            return;
        }

        Logger.LogInformation("Server name: {ServerName}", Settings.Name);
        ServerState = ServerState.Running;
        Tick = 0;

        _runNetworkTask = Task.Run(() => RunNetworkLoop(_ctsServer.Token), _ctsServer.Token);
        _runGameLoopTask = Task.Run(() => RunGameLoop(_ctsServer.Token), _ctsServer.Token);

        await Task.WhenAny(
            _runNetworkTask,
            _runGameLoopTask
        );

        await StopAsync();
    }

    protected virtual void BeforeStop()
    {
    }

    public async Task StopAsync()
    {
        _ctsServer!.Cancel();

        BeforeStop();

        await Task.WhenAll(
            _runNetworkTask!,
            _runGameLoopTask!
        );

        ServerState = ServerState.Stopped;
    }

    protected virtual void RunNetworkLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
        }

        Logger.LogDebug("Network loop stopped");
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
        _settingsChangesListener?.Dispose();
        _ctsServer?.Dispose();
        _ctsServer = null;
    }
}

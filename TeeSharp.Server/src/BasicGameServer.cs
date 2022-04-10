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

public class BasicGameServer : IGameServer, IDisposable
{
    public const int TickRate = 50;

    public int Tick { get; private set; }
    public TimeSpan GameTime { get; private set; }
    public ServerState ServerState { get; private set; }

    public ServerSettings Settings { get; protected set; }

    protected INetworkServer NetworkServer { get; }

    protected TimeSpan AccumulatedElapsedTime { get; private set; }
    protected long PrevTicks { get; private set; }

    private readonly Stopwatch _gameTimer;
    private readonly TimeSpan _targetElapsedTime = TimeSpan.FromTicks(TicksPerSecond / TickRate);
    private readonly TimeSpan _maxElapsedTime = TimeSpan.FromMilliseconds(500);

    private const long TicksPerMillisecond = 10000;
    private const long TicksPerSecond = TicksPerMillisecond * 1000;

    private CancellationTokenSource? _ctsServer;
    private readonly ILogger _logger;

    public BasicGameServer(
        ISettingsChangesNotifier<ServerSettings> serverSettingsNotifier,
        ISettingsChangesNotifier<NetworkServerSettings> networkSettingsNotifier)
    {
        serverSettingsNotifier.Subscribe(OnChangeSettings);

        _logger = Tee.LoggerFactory.CreateLogger("GameServer");
        _gameTimer = new Stopwatch();

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

            _logger.LogInformation("The settings changes have been applied");
        }
        else
        {
            _logger.LogInformation("Hot reload disabled, changes ignored");
        }
    }

    public virtual async Task RunAsync(CancellationToken cancellationToken)
    {
        if (ServerState != ServerState.StartsUp)
        {
            _logger.LogWarning("Already in `Running` state");
            return;
        }

        _ctsServer = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _gameTimer.Start();
        _logger.LogInformation("Run server: {ServerName}", Settings.Name);

        ServerState = ServerState.Running;
        Tick = 0;

        var runResult = await Task.WhenAny(
            Task
                .Run(() => RunNetworkServer(_ctsServer.Token), _ctsServer.Token)
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        _logger.LogError("Network server stopped with ERROR");
                        throw task.Exception!;
                    }

                    _logger.LogInformation("Network server stopped");
                }, CancellationToken.None),
            Task
                .Run(() => RunGameLoop(_ctsServer.Token), _ctsServer.Token)
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        _logger.LogError("Game loop stopped with ERROR");
                        throw task.Exception!;
                    }

                    _logger.LogInformation("Game loop stopped");
                }, CancellationToken.None)
        );

        if (runResult.IsFaulted)
        {
            _ctsServer.Cancel();
            await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None);
            throw runResult.Exception!;
        }

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
        while (!cancellationToken.IsCancellationRequested)
        {
            BeginLoop:

            var currentTicks = _gameTimer.Elapsed.Ticks;
            AccumulatedElapsedTime += TimeSpan.FromTicks(currentTicks - PrevTicks);
            PrevTicks = currentTicks;

            if (AccumulatedElapsedTime < _targetElapsedTime)
            {
                var sleepTime = (_targetElapsedTime - AccumulatedElapsedTime).TotalMilliseconds;
#if _WINDOWS
                ThreadsHelper.SleepForNoMoreThan(sleepTime);
#else
                if (sleepTime >= 2)
                    Thread.Sleep(1);
#endif
                goto BeginLoop;
            }

            if (AccumulatedElapsedTime > _maxElapsedTime)
                AccumulatedElapsedTime = _maxElapsedTime;

            var stepCount = 0;
            while (AccumulatedElapsedTime >= _targetElapsedTime)
            {
                AccumulatedElapsedTime -= _targetElapsedTime;
                GameTime += _targetElapsedTime;

                ++Tick;
                ++stepCount;

                Update();
            }

            if (stepCount > 0)
            {
            }

            // NetworkUpdate();

            if (ServerState == ServerState.Stopping)
                break;
        }
    }

    /// <summary>
    /// Game server tick
    /// </summary>
    protected virtual void Update()
    {
        if (Tick % TickRate == 0)
        {
            _logger.LogInformation("Tick - {Tick}", Tick);
            _logger.LogInformation("Name: {ServerName}", Settings.Name);
        }
    }

    public void Dispose()
    {
        _ctsServer?.Dispose();
        _ctsServer = null;
    }
}

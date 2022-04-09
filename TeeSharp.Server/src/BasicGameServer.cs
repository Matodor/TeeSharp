using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeeSharp.Core;
using TeeSharp.Core.Helpers;
using TeeSharp.Network;

namespace TeeSharp.Server;

public class BasicGameServer : IGameServer, IDisposable
{
    public const int TickRate = 50;

    public int Tick { get; private set; }
    public TimeSpan GameTime { get; private set; }
    public ServerState ServerState { get; private set; }

    public ServerSettings Settings => _settings.Value;

    protected event EventHandler<string>? ServerNameChanged;

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
    private readonly IOptions<ServerSettings> _settings;
    private readonly IOptionsMonitor<ServerSettings> _settingsMonitor;
    private readonly IDisposable? _settingsMonitorListener;

    public BasicGameServer(
        ILoggerFactory loggerFactory,
        INetworkServer networkServer,
        IOptions<ServerSettings> settings,
        IOptionsMonitor<ServerSettings> settingsMonitor)
    {
        _logger = loggerFactory.CreateLogger("GameServer");
        _gameTimer = new Stopwatch();
        _settings = settings;
        _settingsMonitor = settingsMonitor;
        _settingsMonitorListener = _settingsMonitor.OnChange(
            Debouncer.Debounce<ServerSettings, string?>(
                OnChangeSettings,
                TimeSpan.FromSeconds(1)
            )
        );

        NetworkServer = networkServer;
        ServerState = ServerState.StartsUp;
    }

    protected virtual void OnChangeSettings(ServerSettings changedSettings, string? name)
    {
        if (Settings.UseHotReload)
        {
            DetectSettingChanges(changedSettings);
        }
        else
        {
            _logger.LogInformation("Hot reload disabled, changes ignored");
        }
    }

    protected virtual void DetectSettingChanges(ServerSettings changedSettings)
    {
        Settings.UseHotReload = changedSettings.UseHotReload;

        if (Settings.Name != changedSettings.Name)
        {
            Settings.Name = changedSettings.Name;
            ServerNameChanged?.Invoke(this, changedSettings.Name);
        }

        _logger.LogInformation("The settings changes have been applied");
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

        var runTask = await Task.WhenAny(
            Task.Run(() => RunNetworkServerAsync(_ctsServer.Token), _ctsServer.Token),
            Task.Run(() => RunGameLoopAsync(_ctsServer.Token), _ctsServer.Token)
        );

        if (runTask.IsFaulted && !_ctsServer.IsCancellationRequested)
        {
            _ctsServer.Cancel();
            throw runTask.Exception!;
        }
    }

    protected virtual async Task RunNetworkServerAsync(CancellationToken cancellationToken)
    {
        await NetworkServer.RunAsync(cancellationToken);
        _logger.LogInformation("Network server stopped");
    }

    protected virtual Task RunGameLoopAsync(CancellationToken cancellationToken)
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

        _logger.LogInformation("Game loop stopped");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Game server tick
    /// </summary>
    protected virtual void Update()
    {
        if (Tick % TickRate == 0)
        {
            Settings.Name = $"NAME {Tick}";

            _logger.LogInformation("Tick - {Tick}", Tick);
            _logger.LogInformation("Current name: {ServerName}", _settingsMonitor.CurrentValue.Name);
            _logger.LogInformation("Name: {ServerName}", _settings.Value.Name);
        }
    }

    public void Dispose()
    {
        _ctsServer?.Dispose();
        _ctsServer = null;
        _settingsMonitorListener?.Dispose();
    }
}

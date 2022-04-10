using Microsoft.Extensions.Options;
using TeeSharp.Core;
using TeeSharp.Network;
using TeeSharp.Server;

namespace Examples.BasicServer;

public class ServerWorker : BackgroundService
{
    private readonly IOptionsMonitor<ServerSettings> _serverSettingsMonitor;
    private readonly IOptionsMonitor<NetworkServerSettings> _networkSettingsMonitor;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IGameServer _gameServer;

    public ServerWorker(
        ILoggerFactory loggerFactory,
        IOptionsMonitor<ServerSettings> serverSettingsMonitor,
        IOptionsMonitor<NetworkServerSettings> networkSettingsMonitor,
        IHostApplicationLifetime applicationLifetime)
    {
        Tee.Logger = loggerFactory.CreateLogger("TeeSharp");
        Tee.LoggerFactory = loggerFactory;

        _serverSettingsMonitor = serverSettingsMonitor;
        _networkSettingsMonitor = networkSettingsMonitor;
        _applicationLifetime = applicationLifetime;

        _gameServer = new BasicGameServer(
            new SettingsChangesNotifier<ServerSettings>(serverSettingsMonitor),
            new SettingsChangesNotifier<NetworkServerSettings>(networkSettingsMonitor));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _gameServer.RunAsync(stoppingToken);
        _applicationLifetime.StopApplication();
    }

    public override void Dispose()
    {
        base.Dispose();

        _gameServer.Dispose();
    }
}

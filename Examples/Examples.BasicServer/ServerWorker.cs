using Microsoft.Extensions.Options;
using TeeSharp.Core;
using TeeSharp.Server;

namespace Examples.BasicServer;

public class ServerWorker : BackgroundService
{
    private readonly IOptionsMonitor<ServerSettings> _serverSettingsMonitor;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IGameServer _gameServer;

    public ServerWorker(
        ILoggerFactory loggerFactory,
        IOptionsMonitor<ServerSettings> serverSettingsMonitor,
        IHostApplicationLifetime applicationLifetime)
    {
        Tee.Logger = loggerFactory.CreateLogger("TeeSharp");
        Tee.LoggerFactory = loggerFactory;

        _serverSettingsMonitor = serverSettingsMonitor;
        _applicationLifetime = applicationLifetime;

        _gameServer = new BasicGameServer(
            new SettingsChangesNotifier<ServerSettings>(serverSettingsMonitor)
        );
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() =>
        {
            _gameServer.Run(stoppingToken);
            _applicationLifetime.StopApplication();
        }, stoppingToken);
    }

    public override void Dispose()
    {
        base.Dispose();

        _gameServer.Dispose();
    }
}

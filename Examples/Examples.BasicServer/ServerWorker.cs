using Microsoft.Extensions.Options;
using TeeSharp.Core;
using TeeSharp.Server;
using TeeSharp.Server.Abstract;
using TeeSharp.Server.Concrete;

namespace Examples.BasicServer;

public class ServerWorker : BackgroundService
{
    private readonly IOptionsMonitor<ServerSettings> _serverSettingsMonitor;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IServer _server;

    public ServerWorker(
        ILoggerFactory loggerFactory,
        IOptionsMonitor<ServerSettings> serverSettingsMonitor,
        IHostApplicationLifetime applicationLifetime)
    {
        Tee.Logger = loggerFactory.CreateLogger("TeeSharp");
        Tee.LoggerFactory = loggerFactory;

        _serverSettingsMonitor = serverSettingsMonitor;
        _applicationLifetime = applicationLifetime;

        _server = new Server(
            new SettingsChangesNotifier<ServerSettings>(serverSettingsMonitor)
        );
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() =>
        {
            _server.Run(stoppingToken);
            _applicationLifetime.StopApplication();
        }, stoppingToken);
    }

    public override void Dispose()
    {
        base.Dispose();

        _server.Dispose();
    }
}

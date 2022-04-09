using TeeSharp.Server;

namespace Examples.BasicServer;

public class ServerWorker : BackgroundService
{
    private readonly IGameServer _gameServer;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public ServerWorker(
        IGameServer gameServer,
        IHostApplicationLifetime applicationLifetime)
    {
        _gameServer = gameServer;
        _applicationLifetime = applicationLifetime;
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

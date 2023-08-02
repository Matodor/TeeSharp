using System.Net;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using TeeSharp.Core;
using TeeSharp.MasterServer;
using TeeSharp.Network.Concrete;

namespace Examples.MasterServerInteractor;

internal class Program
{
    private readonly NetworkServer _networkServer;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly TeeSharp.MasterServer.MasterServerInteractor _interactor;

    public static Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        Tee.LoggerFactory = new SerilogLoggerFactory(Log.Logger);
        Tee.Logger = Tee.LoggerFactory.CreateLogger("Examples.MasterServerInteractor");

        return new Program().MainAsync(args);
    }

    private Program()
    {
        _networkServer = new NetworkServer();
        _cancellationTokenSource = new CancellationTokenSource();
        _interactor = new TeeSharp.MasterServer.MasterServerInteractor()
        {
            // Endpoint = new Uri("https://master1.ddnet.org/ddnet/15/register"),
            Endpoint = new Uri("http://127.0.0.1:8080/ddnet/15/register"),
        };
    }

    private async Task MainAsync(string[] args)
    {
        if (!InitNetwork())
            return;

        var info = new ServerInfo
        {
            Name = "TeeSharp - test MasterServerInteractor",
            GameType = "TeeSharp",
            HasPassword = true,
            Version = "0.6.4",
            MaxPlayers = 32,
            MaxClients = 32,
            Map =
            {
                Name = "test",
                Size = 128,
                Checksum = "test",
            },
            Clients = new ServerInfoClient[]
            {
                new()
                {
                    Name = "Matodor",
                    IsAfk = true,
                    Team = -1,
                    Clan = "test",
                    Country = 0,
                    IsPlayer = true,
                    Score = 666,
                    Skin = new ServerInfoClientSkin
                    {
                        Name = "pinky",
                        ColorBody = 0,
                        ColorFeet = 0,
                    },
                },
            },
        };

        _interactor.UpdateServerInfo(info);

        Console.CancelKeyPress += (_, e) =>
        {
            _cancellationTokenSource.Cancel();
            e.Cancel = true;
        };

        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            UpdateNetwork();
            Thread.Sleep(20);
        }

        Tee.Logger.LogInformation("shutdown");
        _interactor.Dispose();
    }

    private bool InitNetwork()
    {
        return _networkServer.TryInit(
            localEP: new IPEndPoint(IPAddress.Any, 8303),
            maxConnections: 1,
            maxConnectionsPerIp: 1
        );
    }

    private void UpdateNetwork()
    {
        if (_cancellationTokenSource.IsCancellationRequested)
            return;

        foreach (var message in _networkServer.GetMessages(_cancellationTokenSource.Token))
        {
            if (message.ConnectionId != -1)
                continue;

            if (_interactor.ProcessMasterServerPacket(message.Data, message.EndPoint))
                continue;

            Tee.Logger.LogInformation("{Data}", message.Data.Length);
        }
    }
}

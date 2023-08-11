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
    private readonly CancellationTokenSource _cts;
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
        _cts = new CancellationTokenSource();
        _interactor = new TeeSharp.MasterServer.MasterServerInteractor(_cts.Token)
        {
            Endpoint = new Uri("https://master1.ddnet.org/ddnet/15/register"),
            // Endpoint = new Uri("http://127.0.0.1:8080/ddnet/15/register"),
        };
    }

    private async Task MainAsync(string[] args)
    {
        if (!InitNetwork())
            return;

        _interactor.UpdateServerInfo(GetRandomInfo());

        Console.CancelKeyPress += (_, e) =>
        {
            _cts.Cancel();
            e.Cancel = true;
        };

        _ = Task
            .Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    // ReSharper disable once AccessToDisposedClosure
                    _interactor.UpdateServerInfo(GetRandomInfo());
                    await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(5, 2000)), _cts.Token).ConfigureAwait(false);
                }
            }, _cts.Token)
            .ConfigureAwait(false);

        await Task.Run(() =>
        {
            while (!_cts.IsCancellationRequested)
            {
                Update();
                UpdateNetwork();
                Thread.Sleep(20);
            }
        }, _cts.Token);

        Tee.Logger.LogInformation("Shutdown");

        _interactor.Dispose();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    private bool InitNetwork()
    {
        return _networkServer.TryInit(
            localEP: new IPEndPoint(IPAddress.Any, 8303),
            maxConnections: 1,
            maxConnectionsPerIp: 1
        );
    }

    private void Update()
    {
        _interactor.Update();
    }

    private void UpdateNetwork()
    {
        if (_cts.IsCancellationRequested)
            return;

        foreach (var message in _networkServer.GetMessages(_cts.Token))
        {
            if (message.ConnectionId != -1)
                continue;

            if (_interactor.ProcessMasterServerPacket(message.Data, message.EndPoint))
                continue;

            Tee.Logger.LogInformation("{Data}", message.Data.Length);
        }
    }

    private ServerInfo GetRandomInfo()
    {
        var info = new ServerInfo
        {
            Name = "TeeSharp - test MasterServerInteractor",
            GameType = "TeeSharp",
            HasPassword = true,
            Version = "0.6.4",
            MaxPlayers = 32,
            MaxClients = 32,
            Map = new ServerInfoMap
            {
                Name = "test",
                Size = 128,
                Checksum = "test",
            },
            Clients = Enumerable
                .Range(0, Random.Shared.Next(0, 6))
                .Select(i => new ServerInfoClient
                {
                    Name = $"Matodor_{i}",
                    IsAfk = true,
                    Team = -1,
                    Clan = "test",
                    Country = 0,
                    IsPlayer = true,
                    Score = Random.Shared.Next(0, 999),
                    Skin = new ServerInfoClientSkin
                    {
                        Name = "pinky",
                        ColorBody = Random.Shared.Next(0, 999999),
                        ColorFeet = Random.Shared.Next(0, 999999),
                    },
                }),
        };

        return info;
    }
}

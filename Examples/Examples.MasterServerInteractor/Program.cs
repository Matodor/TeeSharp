using Serilog;
using Serilog.Extensions.Logging;
using TeeSharp.Core;
using TeeSharp.MasterServer;

namespace Examples.MasterServerInteractor;

internal class Program
{
    static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        Tee.LoggerFactory = new SerilogLoggerFactory(Log.Logger);
        Tee.Logger = Tee.LoggerFactory.CreateLogger("Examples.MasterServerInteractor");

        var interactor = new TeeSharp.MasterServer.MasterServerInteractor();

        interactor.UpdateServerInfo(new ServerInfo()
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
                    },
                },
            },
        });
    }
}

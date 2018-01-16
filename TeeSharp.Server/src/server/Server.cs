using System;
using System.Net;
using System.Threading;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Console;
using TeeSharp.Common.Storage;
using TeeSharp.Network;
using TeeSharp.Server.Game;

namespace TeeSharp.Server
{
    public class DefaultServerKernel : IKernelConfig
    {
        public void Load(IKernel kernel)
        {
            // singletons
            kernel.Bind<BaseServer>().To<Server>().AsSingleton();
            kernel.Bind<BaseConfig>().To<ServerConfig>().AsSingleton();
            kernel.Bind<BaseServerBan>().To<ServerBan>().AsSingleton();
            kernel.Bind<BaseGameContext>().To<GameContext>().AsSingleton();
            kernel.Bind<BaseStorage>().To<Storage>().AsSingleton();
            kernel.Bind<BaseNetworkServer>().To<NetworkServer>().AsSingleton();
            kernel.Bind<BaseGameConsole>().To<GameConsole>().AsSingleton();
            kernel.Bind<BaseRegister>().To<Register>().AsSingleton();

            kernel.Bind<BaseServerClient>().To<ServerClient>();
            kernel.Bind<BaseNetworkConnection>().To<NetworkConnection>();
        }
    }

    public class Server : BaseServer
    {
        public override long Tick { get; protected set; }

        protected override BaseServerBan ServerBan { get; set; }
        protected override BaseRegister Register { get; set; }
        protected override BaseGameContext GameContext { get; set; }
        protected override BaseConfig Config { get; set; }
        protected override BaseGameConsole Console { get; set; }
        protected override BaseStorage Storage { get; set; }
        protected override BaseNetworkServer NetworkServer { get; set; }

        protected override BaseServerClient[] Clients { get; set; }
        protected override long StartTime { get; set; }
        protected override bool IsRunning { get; set; }

        public override void Init(string[] args)
        {
            Tick = 0;
            StartTime = 0;
            Clients = new BaseServerClient[MAX_CLIENTS];

            for (var i = 0; i < MAX_CLIENTS; i++)
            {
                Clients[i] = Kernel.Get<BaseServerClient>();
                Clients[i].SnapshotStorage.Init();
            }

            Config = Kernel.Get<BaseConfig>();
            GameContext = Kernel.Get<BaseGameContext>();
            Storage = Kernel.Get<BaseStorage>();
            NetworkServer = Kernel.Get<BaseNetworkServer>();
            Console = Kernel.Get<BaseGameConsole>();
            Register = Kernel.Get<BaseRegister>();
            ServerBan = Kernel.Get<BaseServerBan>();

            if (Config == null ||
                GameContext == null ||
                Storage == null ||
                NetworkServer == null ||
                Console == null)
            {
                throw new Exception("Register components fail");
            }

            Storage.Init("TeeSharp", StorageType.SERVER);

            Console.Init();
            Console.RegisterPrintCallback((OutputLevel) Config["ConsoleOutputLevel"].AsInt(),
                SendRconLineAuthed);

            NetworkServer.Init();

            RegisterCommands();

            Console.ExecuteFile("autoexec.cfg");
            Console.ParseArguments(args);
        }

        public override void Run()
        {
            if (IsRunning)
                return;

            Debug.Log("server", "starting...");

            if (!LoadMap(Config["SvMap"].AsString()))
            {
                Debug.Error("server", $"failed to load map. mapname='{Config["SvMap"]}'");
                return;    
            }

            var bindAddr = IPAddress.Any;
            if (!string.IsNullOrWhiteSpace(Config["Bindaddr"].AsString()))
                bindAddr = IPAddress.Parse(Config["Bindaddr"].AsString());
            var localEP = new IPEndPoint(bindAddr, Config["SvPort"].AsInt());

            if (!NetworkServer.Open(localEP, 
                Config["SvMaxClients"].AsInt(), 
                Config["SvMaxClientsPerIP"].AsInt()))
            {
                Debug.Error("server", $"couldn't open socket. port {localEP.Port} might already be in use");
                return;
            }

            NetworkServer.SetCallbacks(NewClientCallback, DelClientCallback);
            Console.Print(OutputLevel.STANDARD, "server", $"server name is '{Config["SvName"]}'");
            GameContext.OnInit();

            StartTime = Time.Get();
            IsRunning = true;

            while (IsRunning)
            {
                var now = Time.Get();
                var ticks = 0;

                while (now > TickStartTime(Tick + 1))
                {
                    Tick++;
                    ticks++;

                    for (var clientId = 0; clientId < MAX_CLIENTS; clientId++)
                    {
                        if (Clients[clientId].State != ServerClientState.IN_GAME)
                            continue;

                        for (var inputIndex = 0; inputIndex < Clients[clientId].Inputs.Length; inputIndex++)
                        {
                            if (Clients[clientId].Inputs[inputIndex].Tick == Tick)
                            {
                                GameContext.OnClientPredictedInput(clientId,
                                    Clients[clientId].Inputs[inputIndex].Data);
                            }
                        }
                    }

                    GameContext.OnTick();
                }

                if (ticks != 0)
                {
                    if (Tick % 2 == 0 || Config["SvHighBandwidth"].AsBoolean())
                        DoSnapshot();
                    // UpdateClientRconCommands()
                }

                Register.RegisterUpdate(NetworkServer.NetType());
                PumpNetwork();

                Thread.Sleep(5);
            }
        }

        protected override void ProcessClientPacket(NetChunk packet)
        {
            throw new NotImplementedException();
        }

        protected override void PumpNetwork()
        {
            NetworkServer.Update();

            while (NetworkServer.Receive(out var packet))
            {
                if (packet.ClientId == -1)
                {
                    continue;
                }

                ProcessClientPacket(packet);
            }

            ServerBan.Update();
        }

        protected override void DoSnapshot()
        {
        }

        protected override long TickStartTime(long tick)
        {
            return StartTime + (Time.Freq() * tick) / SERVER_TICK_SPEED;

        }

        protected override void DelClientCallback(int clientid, string reason)
        {
            throw new NotImplementedException();
        }

        protected override void NewClientCallback(int clientid)
        {
            throw new NotImplementedException();
        }

        protected override bool LoadMap(string mapName)
        {
            return true;
        }

        protected override void RegisterCommands()
        {
            Console.RegisterCommand("kick", "i?s", ConsoleKick, ConfigFlags.SERVER, "Kick player with specified id for any reason");
            Console.RegisterCommand("status", "", ConsoleStatus, ConfigFlags.SERVER, "List players");
            Console.RegisterCommand("shutdown", "", ConsoleShutdown, ConfigFlags.SERVER, "Shut down");
            Console.RegisterCommand("logout", "", ConsoleLogout, ConfigFlags.SERVER, "Logout of rcon");
            Console.RegisterCommand("reload", "", ConsoleReload, ConfigFlags.SERVER, "Reload the map");

            /*
                Console()->Register("kick", "i?r", CFGFLAG_SERVER, ConKick, this, "Kick player with specified id for any reason");
	            Console()->Register("status", "", CFGFLAG_SERVER, ConStatus, this, "List players");
	            Console()->Register("shutdown", "", CFGFLAG_SERVER, ConShutdown, this, "Shut down");
	            Console()->Register("logout", "", CFGFLAG_SERVER, ConLogout, this, "Logout of rcon");

	            Console()->Register("record", "?s", CFGFLAG_SERVER|CFGFLAG_STORE, ConRecord, this, "Record to a file");
	            Console()->Register("stoprecord", "", CFGFLAG_SERVER, ConStopRecord, this, "Stop recording");

	            Console()->Register("reload", "", CFGFLAG_SERVER, ConMapReload, this, "Reload the map");

	            Console()->Chain("sv_name", ConchainSpecialInfoupdate, this);
	            Console()->Chain("password", ConchainSpecialInfoupdate, this);

	            Console()->Chain("sv_max_clients_per_ip", ConchainMaxclientsperipUpdate, this);
	            Console()->Chain("mod_command", ConchainModCommandUpdate, this);
	            Console()->Chain("console_output_level", ConchainConsoleOutputLevelUpdate, this);
            */

            GameContext.RegisterCommands();
        }

        protected override void ConsoleReload(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        protected override void ConsoleLogout(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        protected override void ConsoleShutdown(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        protected override void ConsoleStatus(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        protected override void ConsoleKick(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        protected override void SendRconLineAuthed(string message, object data)
        {
            
        }
    }
}

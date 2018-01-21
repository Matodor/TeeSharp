using System;
using System.Net;
using System.Threading;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Storage;
using TeeSharp.Core;
using TeeSharp.MasterServer;
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
            kernel.Bind<BaseNetworkBan>().To<NetworkBan>().AsSingleton();
            kernel.Bind<BaseGameContext>().To<GameContext>().AsSingleton();
            kernel.Bind<BaseStorage>().To<Storage>().AsSingleton();
            kernel.Bind<BaseNetworkServer>().To<NetworkServer>().AsSingleton();
            kernel.Bind<BaseGameConsole>().To<GameConsole>().AsSingleton();
            kernel.Bind<BaseRegister>().To<Register>().AsSingleton();

            kernel.Bind<BaseServerClient>().To<ServerClient>();
            kernel.Bind<BaseNetworkConnection>().To<NetworkConnection>();
            kernel.Bind<BaseChunkReceiver>().To<ChunkReceiver>();
        }
    }

    public class Server : BaseServer
    {
        public override long Tick { get; protected set; }

        protected override BaseNetworkBan NetworkBan { get; set; }
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
            NetworkBan = Kernel.Get<BaseNetworkBan>();

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

            var networkConfig = new NetworkServerConfig
            {
                LocalEndPoint = new IPEndPoint(bindAddr, Config["SvPort"].AsInt()),
                ConnectionTimeout = Config["ConnTimeout"].AsInt(),
                MaxClientsPerIp = Config["SvMaxClientsPerIP"].AsInt(),
                MaxClients = Config["SvMaxClients"].AsInt()
            };

            if (!NetworkServer.Open(networkConfig))
            {
                Debug.Error("server", $"couldn't open socket. port {networkConfig.LocalEndPoint.Port} might already be in use");
                return;
            }
            Debug.Log("server", $"network server running at: {networkConfig.LocalEndPoint}");

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

            for (var i = 0; i < Clients.Length; i++)
            {
                if (Clients[i].State != ServerClientState.EMPTY)
                    NetworkServer.Drop(i, "Server shutdown");
            }

            GameContext.OnShutdown();
        }

        protected override void ProcessClientPacket(NetworkChunk packet)
        {
            var clientId = packet.ClientId;
            var unpacker = new Unpacker();
            unpacker.Reset(packet.Data, packet.DataSize);

            var msg = unpacker.GetInt();
            var sys = msg & 1;
            msg >>= 1;

            if (unpacker.Error)
                return;

            var message = (NetworkMessages) msg;
            if (Config["SvNetlimit"].AsBoolean() && message != NetworkMessages.REQUEST_MAP_DATA)
            {
                var now = Time.Get();
                var diff = now - Clients[clientId].TrafficSince;
                var alpha = Config["SvNetlimitAlpha"].AsInt() / 100f;
                var limit = (float) Config["SvNetlimit"].AsInt() * 1024 / Time.Freq();

                if (Clients[clientId].Traffic > limit)
                {
                    NetworkBan.BanAddr(packet.EndPoint, 600, "Stressing network");
                    return;
                }

                if (diff > 100)
                {
                    Clients[clientId].Traffic = (long) (alpha * ((float) packet.DataSize / diff) +
                                                       (1.0f - alpha) * Clients[clientId].Traffic);
                    Clients[clientId].TrafficSince = now;
                }
            }

            if (sys != 0)
            {
                switch (message)
                {
                    case NetworkMessages.INFO:
                        NetMsgInfo(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.REQUEST_MAP_DATA:
                        NetMsgRequestMapData(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.READY:
                        NetMsgReady(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.ENTERGAME:
                        NetMsgEnterGame(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.INPUT:
                        NetMsgInput(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.RCON_CMD:
                        NetMsgRconCmd(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.RCON_AUTH:
                        NetMsgRconAuth(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.PING:
                        NetMsgPing(packet, unpacker, clientId);
                        break;
                    default:
                        Console.Print(OutputLevel.DEBUG, "server", $"strange message clientId={clientId} msg={msg} data_size={packet.DataSize}");
                        break;
                }
            }
            else
            {
                if (Clients[clientId].State >= ServerClientState.READY)
                {
                    GameContext.OnMessage(message, unpacker, clientId);
                }
            }
        }

        protected override void NetMsgPing(NetworkChunk packet, Unpacker unpacker, int clientId)
        {
            throw new NotImplementedException();
        }

        protected override void NetMsgRconAuth(NetworkChunk packet, Unpacker unpacker, int clientId)
        {
            throw new NotImplementedException();
        }

        protected override void NetMsgRconCmd(NetworkChunk packet, Unpacker unpacker, int clientId)
        {
            throw new NotImplementedException();
        }

        protected override void NetMsgInput(NetworkChunk packet, Unpacker unpacker, int clientId)
        {
            throw new NotImplementedException();
        }

        protected override void NetMsgEnterGame(NetworkChunk packet, Unpacker unpacker, int clientId)
        {
            throw new NotImplementedException();
        }

        protected override void NetMsgReady(NetworkChunk packet, Unpacker unpacker, int clientId)
        {
            throw new NotImplementedException();
        }

        protected override void NetMsgRequestMapData(NetworkChunk packet, Unpacker unpacker, int clientId)
        {
            throw new NotImplementedException();
        }

        protected override void NetMsgInfo(NetworkChunk packet, Unpacker unpacker, int clientId)
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
                    if (packet.DataSize == MasterServerPackets.SERVERBROWSE_GETINFO.Length + 1 &&
                        packet.Data.ArrayCompare(MasterServerPackets.SERVERBROWSE_GETINFO))
                    {
                        SendServerInfo(
                            packet.EndPoint, 
                            packet.Data[MasterServerPackets.SERVERBROWSE_GETINFO.Length],
                            false
                        );
                    }
                    else if (packet.DataSize == MasterServerPackets.SERVERBROWSE_GETINFO64.Length + 1 &&
                             packet.Data.ArrayCompare(MasterServerPackets.SERVERBROWSE_GETINFO64))
                    {
                        SendServerInfo(
                            packet.EndPoint,
                            packet.Data[MasterServerPackets.SERVERBROWSE_GETINFO64.Length],
                            true
                        );
                    }

                    continue;
                }

                ProcessClientPacket(packet);
            }

            NetworkBan.Update();
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
            Clients[clientid].State = ServerClientState.AUTH;
            Clients[clientid].PlayerName = string.Empty;
            Clients[clientid].PlayerClan = string.Empty;
            Clients[clientid].PlayerCountry = -1;

            Clients[clientid].Reset();
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

        protected override void SendServerInfo(IPEndPoint endPoint, int token, bool showMore, int offset = 0)
        {
            throw new NotImplementedException();
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

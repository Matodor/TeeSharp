using System;
using System.Net;
using System.Threading;

namespace TeeSharp.Server
{
    public class Server : IServer
    {
        public long Tick => _currentGameTick;
        public bool IsRunning;

        protected IGameConsole _gameConsole;
        protected IGameContext _gameContext;
        protected IEngineMap _map;
        protected IStorage _storage;
        protected INetworkServer _networkServer;
        protected Configuration _config;

        protected IServerClient[] _clients;
        protected long _currentGameTick;
        protected long _gameStartTime;

        public virtual bool LoadMap(string mapName)
        {
            return true;
        }

        public ServerClient GetClient(int clientId)
        {
            throw new NotImplementedException();
        }

        public virtual void RegisterCommands()
        {
            _gameConsole.RegisterCommand("kick", "int", ConfigFlags.SERVER, ConsoleKick, this, "Kick player with specified id for any reason");
            _gameConsole.RegisterCommand("ban", "string", ConfigFlags.SERVER | ConfigFlags.STORE, ConsoleBan, this, "Ban player with ip/id for x minutes for any reason");
            _gameConsole.RegisterCommand("unban", "string", ConfigFlags.SERVER | ConfigFlags.STORE, ConsoleUnBan, this, "Unban ip");
            _gameConsole.RegisterCommand("bans", "", ConfigFlags.SERVER | ConfigFlags.STORE, ConsoleBans, this, "Show banlist");
            _gameConsole.RegisterCommand("status", "", ConfigFlags.SERVER, ConsoleStatus, this, "List players");
            _gameConsole.RegisterCommand("shutdown", "", ConfigFlags.SERVER, ConsoleShutdown, this, "Shut down");
            _gameConsole.RegisterCommand("reload", "", ConfigFlags.SERVER, ConsoleMapReload, this, "Reload the map");
            
            _gameConsole.OnExecuteCommand("sv_name", SpecialInfoUpdate);
            _gameConsole.OnExecuteCommand("password", SpecialInfoUpdate);
            _gameConsole.OnExecuteCommand("sv_max_clients_per_ip", MaxClientsPerIpUpdate);
            _gameConsole.OnExecuteCommand("mod_command", ModCommandUpdate);
            _gameConsole.OnExecuteCommand("console_output_level", ConsoleOutputLevelUpdate);

            // register console commands in sub parts
            //m_ServerBan.InitServerBan(Console(), Storage(), this);
            //GameContext.Instance.OnConsoleInit();
        }

        protected virtual void ConsoleOutputLevelUpdate(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ModCommandUpdate(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void MaxClientsPerIpUpdate(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void SpecialInfoUpdate(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleMapReload(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleShutdown(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleStatus(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleBans(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleUnBan(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleBan(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleKick(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        protected virtual long TickStartTime(long tick)
        {
            return _gameStartTime + (Base.TimeFreq() * tick) / Consts.SERVER_TICK_SPEED;
        }

        protected virtual void DefaultBindinds()
        {
            if (!Kernel.IsBinded<IServerClient>()) Kernel.Bind<IServerClient, ServerClient>();
            if (!Kernel.IsBinded<IPlayer>()) Kernel.Bind<IPlayer, Player>();

            // bind singletons
            if (!Kernel.IsBinded<Configuration>()) Kernel.Bind<Configuration, Configuration>(new Configuration());
            if (!Kernel.IsBinded<IGameContext>()) Kernel.Bind<IGameContext, GameContext>(new GameContext());
            if (!Kernel.IsBinded<IEngineMap>()) Kernel.Bind<IEngineMap, Map>(new Map());
            if (!Kernel.IsBinded<IStorage>()) Kernel.Bind<IStorage, Storage>(new Storage());
            if (!Kernel.IsBinded<INetworkServer>()) Kernel.Bind<INetworkServer, NetworkServer>(new NetworkServer());
            if (!Kernel.IsBinded<IGameConsole>()) Kernel.Bind<IGameConsole, GameConsole>(new GameConsole());
        }

        public virtual void Init(string[] args)
        {
            DefaultBindinds();

            _currentGameTick = 0;
            _clients = new IServerClient[Consts.MAX_CLIENTS];

            for (var i = 0; i < _clients.Length; i++)
                _clients[i] = Kernel.Get<IServerClient>();

            _config = Kernel.Get<Configuration>();
            _gameContext = Kernel.Get<IGameContext>();
            _map = Kernel.Get<IEngineMap>();
            _storage = Kernel.Get<IStorage>();
            _networkServer = Kernel.Get<INetworkServer>();
            _gameConsole = Kernel.Get<IGameConsole>();

            var registerFail = false;
            registerFail = registerFail || _gameContext == null;
            registerFail = registerFail || _map == null;
            registerFail = registerFail || _storage == null;
            registerFail = registerFail || _networkServer == null;
            registerFail = registerFail || _config == null;
            registerFail = registerFail || _gameConsole == null;

            if (registerFail)
                throw new Exception("Register components fail");

            _storage.Init("Teeworlds");
            _gameConsole.Init();
            _networkServer.Init();

            // register all console commands
            RegisterCommands();

            // execute autoexec file
            _gameConsole.ExecuteFile("autoexec.cfg");
            _gameConsole.ParseArguments(args);
        }

        protected virtual void SendRconLineAuthed(string str)
        {
            
        }

        protected virtual void ProcessClientPacket(NetChunk packet)
        {
            
        }

        protected virtual void PumpNetwork()
        {
            _networkServer.Update();

            NetChunk packet;
            while (_networkServer.Receive(out packet))
            {
                if (packet.ClientId == -1)
                {
                    continue;   
                }

                ProcessClientPacket(packet);
            }

            // server ban update
            // econ update
        }

        public virtual void Run()
        {
            if (IsRunning)
                return;

            Base.DbgMessage("server", "starting...", ConsoleColor.Red);
#if DEBUG
            Base.DbgMessage("server", "running on debug version", ConsoleColor.Red);
#endif

            _gameConsole.RegisterPrintCallback((ConsoleOutputLevel) _config.GetInt("ConsoleOutputLevel"),
                SendRconLineAuthed, null);

            // load map
            if (!LoadMap(_config.GetString("SvMap")))
            {
                Base.DbgMessage("server", $"failed to load map. mapname='{_config.GetString("SvMap")}'");
                return;
            }

            _networkServer.Open(new IPEndPoint(IPAddress.Any, _config.GetInt("SvPort")),
                _config.GetInt("SvMaxClients"), _config.GetInt("SvMaxClientsPerIP"));
            _networkServer.SetCallbacks(NewClientCallback, DelClientCallback);

            _gameConsole.Print(ConsoleOutputLevel.STANDARD, "server", $"server name is '{_config.GetString("SvName")}'");
            _gameContext.OnInit();

            _gameStartTime = Base.TimeGet();
            IsRunning = true;

            var time = 0L;
            var ticks = 0;

            while (IsRunning)
            {
                time = Base.TimeGet();
                ticks = 0;

                while (time > TickStartTime(_gameStartTime + 1))
                {
                    _currentGameTick++;
                    ticks++;

                    for (var c = 0; c < Consts.MAX_CLIENTS; c++)
                    {
                        if (_clients[c].ClientState != ServerClientState.INGAME)
                            continue;

                        /*for (var i = 0; i < 200; i++)
                        {
                            if (_clients[c].m_aInputs[i].m_GameTick == Tick)
                            {
                                gameServer.OnClientPredictedInput(c, _clients[c].m_aInputs[i].m_aData);
                                break;
                            }
                        }*/
                    }

                    _gameContext.OnTick();
                }

                if (ticks != 0)
                {
                    if (_currentGameTick % 2 == 0 || _config.GetInt("SvHighBandwidth") != 0)
                        DoSnapshot();
                }

                PumpNetwork();

                Thread.Sleep(5);
            }

            for (var i = 0; i < Consts.MAX_CLIENTS; ++i)
            {
                if (_clients[i].ClientState != ServerClientState.EMPTY)
                    _networkServer.Drop(i, "Server shutdown");
            }

            _gameContext.OnShutdown();
            
        }

        private void SendRconLineAuthed(string str, object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void DoSnapshot()
        {
            throw new NotImplementedException();
        }

        protected virtual void DelClientCallback(int clientId, string reason)
        {
            _clients[clientId].ClientState = ServerClientState.AUTH;
        }

        protected virtual void NewClientCallback(int clientId)
        {
            _clients[clientId].ClientState = ServerClientState.EMPTY;
        }
    }
}

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

        protected readonly Client[] _clients;
        protected long _currentGameTick;
        protected long _gameStartTime;

        public Server()
        {
            _currentGameTick = 0;
            _clients = new Client[Consts.MAX_CLIENTS];

            for (var i = 0; i < _clients.Length; i++)
            {
                _clients[i] = new Client()
                {
                    
                };
            }
        }

        public virtual bool LoadMap(string mapName)
        {
            return true;
        }

        public Client GetClient(int clientId)
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
            return _gameStartTime + (System.TimeFreq() * tick) / Consts.SERVER_TICK_SPEED;
        }

        public virtual void Init(string[] args)
        {
            _config = Kernel.Get<Configuration>() ?? Kernel.BindGet<Configuration, Configuration>(new Configuration());
            _gameContext = Kernel.Get<IGameContext>()     ?? Kernel.BindGet<IGameContext, GameContext>(new GameContext());
            _map = Kernel.Get<IEngineMap>()               ?? Kernel.BindGet<IEngineMap, Map>(new Map());
            _storage = Kernel.Get<IStorage>()             ?? Kernel.BindGet<IStorage, Storage>(new Storage());
            _networkServer = Kernel.Get<INetworkServer>() ?? Kernel.BindGet<INetworkServer, NetworkServer>(new NetworkServer());
            _gameConsole = Kernel.Get<IGameConsole>()     ?? Kernel.BindGet<IGameConsole, GameConsole>(new GameConsole());

            var registerFail = false;
            registerFail = registerFail || _gameContext == null;
            registerFail = registerFail || _map == null;
            registerFail = registerFail || _storage == null;
            registerFail = registerFail || _networkServer == null;
            registerFail = registerFail || _config == null;
            registerFail = registerFail || _gameConsole == null;

            if (registerFail)
                throw new Exception("Register components fail");

            _gameConsole.Init();
            _networkServer.Init();

            // register all console commands
            RegisterCommands();

            // execute autoexec file
            _gameConsole.ExecuteFile("autoexec.cfg");
            _gameConsole.ParseArguments(args);
        }

        public virtual void Run()
        {
            if (IsRunning)
                return;

            System.DbgMessage("server", "starting...", ConsoleColor.Red);
#if DEBUG
            System.DbgMessage("server", "running on debug version", ConsoleColor.Red);
#endif

            // load map
            if (!LoadMap(_config.GetString("SvMap")))
            {
                System.DbgMessage("server", $"failed to load map. mapname='{_config.GetString("SvMap")}'");
                return;
            }

            _networkServer.Open(new IPEndPoint(IPAddress.Any, _config.GetInt("SvPort")),
                _config.GetInt("SvMaxClients"), _config.GetInt("SvMaxClientsPerIP"));
            _networkServer.SetCallbacks(NewClientCallback, DelClientCallback);

            _gameConsole.Print(ConsoleOutputLevel.STANDARD, "server", $"server name is '{_config.GetString("SvName")}'");
            _gameContext.OnInit();

            _gameStartTime = System.TimeGet();
            IsRunning = true;

            var time = 0L;
            var ticks = 0;

            while (IsRunning)
            {
                time = System.TimeGet();
                ticks = 0;

                while (time > TickStartTime(_gameStartTime + 1))
                {
                    _currentGameTick++;
                    ticks++;

                    // TODO
                    /*for (var c = 0; c < Consts.MAX_CLIENTS; c++)
                    {
                        if (_clients[c].ClientState != ClientState.STATE_INGAME)
                            continue;
                        for (var i = 0; i < 200; i++)
                        {
                            if (_clients[c].m_aInputs[i].m_GameTick == Tick)
                            {
                                gameServer.OnClientPredictedInput(c, _clients[c].m_aInputs[i].m_aData);
                                break;
                            }
                        }
                    }*/

                    _gameContext.OnTick();
                }

                Thread.Sleep(5);
            }    
        }

        protected virtual void DelClientCallback(int clientId, string reason)
        {
            throw new NotImplementedException();
        }

        protected virtual void NewClientCallback(int clientId)
        {
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Threading;

namespace TeeSharp.Server
{
    public class Server : IServer
    {
        public ulong Tick => _currentGameTick;
        public bool IsRunning;

        protected IGameConsole _gameConsole;
        protected IGameContext _gameContext;
        protected IEngineMap _map;
        protected IStorage _storage;
        protected INetworkServer _networkServer;
        protected Configuration _config;

        protected readonly Client[] _clients;
        protected ulong _currentGameTick;

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

            //_gameConsole.RegisterCommand("record", "?s", ConfigFlags.SERVER | ConfigFlags.STORE, ConRecord, this, "Record to a file");
            //_gameConsole.RegisterCommand("stoprecord", "", ConfigFlags.SERVER, ConStopRecord, this, "Stop recording");
            
            _gameConsole.OnExecuteCommand("sv_name", SpecialInfoUpdate);
            _gameConsole.OnExecuteCommand("password", SpecialInfoUpdate);

            _gameConsole.OnExecuteCommand("sv_max_clients_per_ip", MaxClientsPerIpUpdate);
            _gameConsole.OnExecuteCommand("mod_command", ModCommandUpdate);
            _gameConsole.OnExecuteCommand("console_output_level", ConsoleOutputLevelUpdate);

            // register console commands in sub parts
            //m_ServerBan.InitServerBan(Console(), Storage(), this);
            //GameContext.Instance.OnConsoleInit();
        }

        private void ConsoleOutputLevelUpdate(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        private void ModCommandUpdate(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        private void MaxClientsPerIpUpdate(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        private void SpecialInfoUpdate(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        private void ConsoleMapReload(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        private void ConsoleShutdown(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        private void ConsoleStatus(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        private void ConsoleBans(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        private void ConsoleUnBan(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        private void ConsoleBan(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        private void ConsoleKick(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        public virtual void Init(string[] args)
        {
            _gameContext = Kernel.Get<IGameContext>()     ?? Kernel.BindGet<IGameContext, GameContext>(new GameContext());
            _map = Kernel.Get<IEngineMap>()               ?? Kernel.BindGet<IEngineMap, Map>(new Map());
            _storage = Kernel.Get<IStorage>()             ?? Kernel.BindGet<IStorage, Storage>(new Storage());
            _networkServer = Kernel.Get<INetworkServer>() ?? Kernel.BindGet<INetworkServer, NetworkServer>(new NetworkServer());
            _config = Kernel.Get<Configuration>()         ?? Kernel.BindGet<Configuration, Configuration>(new Configuration());
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

            if (!LoadMap(_config.GetString("SvMap")))
            {
                System.DbgMessage("server", $"failed to load map. mapname='{_config.GetString("SvMap")}'");
                return;
            }

            IsRunning = true;

            while (IsRunning)
            {
                Thread.Sleep(5);
            }    
        }
    }
}

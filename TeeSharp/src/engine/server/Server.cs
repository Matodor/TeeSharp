using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp.Server
{
    public class Server : IServer
    {
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

        public virtual void RegisterCommands()
        {
        }

        public virtual void Init(string[] args)
        {
            _gameContext = Kernel.Get<IGameContext>()     ?? Kernel.BindGet<IGameContext>(new GameContext());
            _map = Kernel.Get<IEngineMap>()               ?? Kernel.BindGet<IEngineMap>(new Map());
            _storage = Kernel.Get<IStorage>()             ?? Kernel.BindGet<IStorage>(new Storage());
            _networkServer = Kernel.Get<INetworkServer>() ?? Kernel.BindGet<INetworkServer>(new NetworkServer());
            _config = Kernel.Get<Configuration>()         ?? Kernel.BindGet<Configuration>(new Configuration());
            _gameConsole = Kernel.Get<IGameConsole>()     ?? Kernel.BindGet<IGameConsole>(new GameConsole());

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
                
            }    
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeeSharp.Server
{
    public class Server : ISingleton
    {
        public bool IsRunning;

        protected IGameContext _gameContext;
        protected IEngineMap _map;
        protected IStorage _storage;
        protected INetworkServer _networkServer;
        protected Configuration _config;

        protected Server()
        {
        }

        protected virtual void Init(Kernel kernel)
        {
            kernel.RegisterSingleton<Server>(this);

            var gameContext = kernel.RequestSingleton<IGameContext>() ??
                kernel.RegisterSingleton<IGameContext>(GameContext.Create());
            var mapEngine = kernel.RequestSingleton<IEngineMap>() ??
                kernel.RegisterSingleton<IEngineMap>(Map.Create());
            var storage = kernel.RequestSingleton<IStorage>() ??
                kernel.RegisterSingleton<IStorage>(Storage.Create("Teeworlds", IStorage.StorageType.SERVER));
            var networkServer = kernel.RequestSingleton<INetworkServer>() ??
                kernel.RegisterSingleton<INetworkServer>(NetworkServer.Create());
            var configuration = kernel.RequestSingleton<Configuration>() ??
                kernel.RegisterSingleton<Configuration>(Configuration.Create());

            var registerFail = false;
            registerFail = registerFail || gameContext == null;
            registerFail = registerFail || mapEngine == null;
            registerFail = registerFail || storage == null;
            registerFail = registerFail || networkServer == null;
            registerFail = registerFail || configuration == null;

            if (registerFail)
                throw new Exception("Register components fail");
        }

        public static Server Create(Kernel kernel = null)
        {
            var server = new Server();
            server.Init(kernel ?? Kernel.Create());
            return server;
        }

        public static T Create<T>(Kernel kernel = null) where T : Server, new()
        {
            var server = new T();
            server.Init(kernel ?? Kernel.Create());
            return server;
        }

        protected virtual bool LoadMap(string mapName)
        {
            return true;
        }

        public virtual void Run()
        {
            if (IsRunning)
                return;

            System.DbgMessage("server", "starting...");
#if DEBUG
            System.DbgMessage("server", "running on debug version", ConsoleColor.Red);
#endif

            _gameContext   = Kernel.RequestSingleton<IGameContext>();
            _map           = Kernel.RequestSingleton<IEngineMap>();
            _storage       = Kernel.RequestSingleton<IStorage>();
            _networkServer = Kernel.RequestSingleton<INetworkServer>();
            _config        = Kernel.RequestSingleton<Configuration>();

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

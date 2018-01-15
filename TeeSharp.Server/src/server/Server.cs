using System;
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
            kernel.Bind<BaseGameContext>().To<GameContext>().AsSingleton();
            kernel.Bind<BaseStorage>().To<Storage>().AsSingleton();
            kernel.Bind<BaseNetworkServer>().To<NetworkServer>().AsSingleton();
            kernel.Bind<BaseGameConsole>().To<GameConsole>().AsSingleton();
            kernel.Bind<BaseRegister>().To<Register>().AsSingleton();

            kernel.Bind<BaseServerClient>().To<ServerClient>();
        }
    }

    public class Server : BaseServer
    {
        public override long Tick { get; protected set; }

        protected override BaseRegister Register { get; set; }
        protected override BaseGameContext GameContext { get; set; }
        protected override BaseConfig Config { get; set; }
        protected override BaseGameConsole Console { get; set; }
        protected override BaseStorage Storage { get; set; }
        protected override BaseNetworkServer NetworkServer { get; set; }

        protected override BaseServerClient[] Clients { get; set; }
        protected override long StartTick { get; set; }
        protected override bool IsRunning { get; set; }

        public override void Init(string[] args)
        {
            Tick = 0;
            StartTick = 0;
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
            NetworkServer.Init();

            RegisterCommands();

            Console.ExecuteFile("autoexec.cfg");
            Console.ParseArguments(args);
        }

        public override void Run()
        {
            if (IsRunning)
                return;

            IsRunning = true;
            Debug.Log("server", "starting...");

            while (IsRunning)
            {
                
            }
        }

        protected override void RegisterCommands()
        {
        }
    }
}

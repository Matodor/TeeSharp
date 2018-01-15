using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Console;
using TeeSharp.Common.Storage;
using TeeSharp.Network;
using TeeSharp.Server.Game;

namespace TeeSharp.Server
{
    public abstract class BaseServer : BaseInterface
    {
        public const int MAX_CLIENTS = 64;

        public abstract long Tick { get; protected set; }

        protected abstract BaseRegister Register { get; set; }
        protected abstract BaseGameContext GameContext { get; set; }
        protected abstract BaseConfig Config { get; set; }
        protected abstract BaseGameConsole Console { get; set; }
        protected abstract BaseStorage Storage { get; set; }
        protected abstract BaseNetworkServer NetworkServer { get; set; }

        protected abstract BaseServerClient[] Clients { get; set; }
        protected abstract long StartTick { get; set; }
        protected abstract bool IsRunning { get; set; }

        public abstract void Init(string[] args);
        public abstract void Run();
        protected abstract void RegisterCommands();
    }
}
using System;
using System.Net;
using TeeSharp.Common.Config;
using TeeSharp.Common.Storage;
using TeeSharp.Core.MinIoC;
using TeeSharp.MasterServer;
using TeeSharp.Network;

namespace TeeSharp.Server
{
    public abstract class BaseServer : IServiceBinder, IContainerService
    {
        /// <summary>
        /// 50 Ticks per second
        /// </summary>
        public const int TickRate = 50;

        public Container Container { get; set; }
        public TimeSpan GameTime { get; protected set; }
        public ServerState ServerState { get; protected set; }
        public abstract int Tick { get; protected set; }

        protected BaseNetworkServer NetworkServer { get; set; }
        protected BaseConfiguration Config { get; set; }
        protected BaseStorage Storage { get; set; }

        public abstract void Init();
        public abstract void Run();
        public abstract void Stop();
        public abstract void ConfigureServices(Container services);
        public abstract void SendServerInfo(ServerInfoType type, 
            IPEndPoint addr, SecurityToken token);
        
        protected abstract void ProcessNetworkMessage(
            NetworkMessage msg, SecurityToken responseToken);
        protected abstract void ProcessMasterServerMessage(
            NetworkMessage msg, SecurityToken responseToken);
        protected abstract void ProcessClientMessage(
            NetworkMessage msg, SecurityToken responseToken);
    }
}
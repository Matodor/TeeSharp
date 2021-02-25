using System;
using TeeSharp.Core.MinIoC;
using TeeSharp.Network;

namespace TeeSharp.Server
{
    public abstract class BaseServer : IServiceBinder, IContainerService
    {
        public const int TickRate = 50;
        
        public Container Container { get; set; }
        public TimeSpan GameTime { get; protected set; }
        public ServerState ServerState { get; protected set; }
        public abstract int Tick { get; protected set; }
        
        protected BaseNetworkServer NetworkServer { get; set; }
        
        public abstract void Init();
        public abstract void Run();
        public abstract void Stop();
        public abstract void ConfigureServices(Container services);
    }
}
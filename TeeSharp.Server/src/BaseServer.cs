using System;
using System.Diagnostics;
using TeeSharp.Network;

namespace TeeSharp.Server
{
    public abstract class BaseServer
    {
        public const int TickRate = 50;
        
        public TimeSpan GameTime { get; protected set; }
        public ServerState ServerState { get; protected set; }
        public abstract int Tick { get; protected set; }
        
        protected BaseNetworkServer NetworkServer { get; set; }
        
        public abstract void Init();
        public abstract void Run();
    }
}
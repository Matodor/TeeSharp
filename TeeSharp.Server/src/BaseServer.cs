using System;
using System.Diagnostics;

namespace TeeSharp.Server
{
    public abstract class BaseServer
    {
        public const int TickRate = 50;
        public const int TickTime = 1000 / TickRate;
        
        public TimeSpan GameTime { get; protected set; }
        public ServerState ServerState { get; protected set; }
        public abstract int Tick { get; protected set; }
        
        public abstract void Init();
        public abstract void Run();
        public abstract void Stop();
    }
}
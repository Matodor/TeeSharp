using System;
using System.Diagnostics;
using System.Threading;
using TeeSharp.Network;
using TeeSharp.Core.Helpers;

namespace TeeSharp.Server
{
    public class DefaultServer : BaseServer
    {
        public override int Tick { get; protected set; }

        protected Stopwatch GameTimer { get; set; }

        protected const long TicksPerMillisecond = 10000;
        protected const long TicksPerSecond = TicksPerMillisecond * 1000;
        
        protected readonly TimeSpan TargetElapsedTime = TimeSpan.FromTicks(TicksPerSecond / TickRate);
        protected readonly TimeSpan MaxElapsedTime = TimeSpan.FromMilliseconds(500);
        protected TimeSpan AccumulatedElapsedTime;
        protected long PrevTicks = 0;
        
        public override void Init()
        {
            if (ServerState >= ServerState.StartsUp)
                return;
            
            ServerState = ServerState.StartsUp;
            
            NetworkServer = new NetworkServer();
            NetworkServer.Init();
            
            
        }

        public override void Run()
        {
            if (ServerState != ServerState.Running)
                ServerState = ServerState.Running;
            else
            {
                // TODO: log already running
                return;    
            }
            
            GameTimer = Stopwatch.StartNew();
            Tick = 0;

            RunNetworkServer();
            RunLoop();
        }
        
        public override void Stop()
        {
            ServerState = ServerState.Stopping;
        }

        protected virtual void RunNetworkServer()
        {
            
        }
        
        protected virtual void RunLoop()
        {
            while (true)
            {
                BeginLoop:
                
                var currentTicks = GameTimer.ElapsedTicks;
                AccumulatedElapsedTime += TimeSpan.FromTicks(currentTicks - PrevTicks);
                PrevTicks = currentTicks;

                if (AccumulatedElapsedTime < TargetElapsedTime)
                {
                    var sleepTime = (TargetElapsedTime - AccumulatedElapsedTime).TotalMilliseconds;
                    
#if _WINDOWS 
                    ThreadsHelper.SleepForNoMoreThan(sleepTime);
#else
                    if (sleepTime >= 2)
                        Thread.Sleep(1);
#endif
                    goto BeginLoop;
                }

                if (AccumulatedElapsedTime > MaxElapsedTime)
                    AccumulatedElapsedTime = MaxElapsedTime;

                var stepCount = 0;
                while (AccumulatedElapsedTime >= TargetElapsedTime)
                {
                    AccumulatedElapsedTime -= TargetElapsedTime;
                    GameTime += TargetElapsedTime;
                    
                    ++Tick;
                    ++stepCount;
                    
                    Update();
                }

                if (stepCount > 0)
                {
                    
                }
                
                if (ServerState == ServerState.Stopping)
                    break;
            }
        }


        protected virtual void UpdateNetwork()
        {
            NetworkServer.Update();
        }
        
        protected virtual void Update()
        {
            if (Tick % TickRate == 0)
                Console.WriteLine($"[{GameTimer.Elapsed:G}] Tick: {Tick}");
        }
    }
}
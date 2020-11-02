using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using TeeSharp.Network;
using TeeSharp.Core.Helpers;
using TeeSharp.Core.MinIoC;

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
        
        protected CancellationTokenSource NetworkLoopCancellationToken { get; set; }
        protected Thread NetworkLoopThread { get; set; }
        
        public override void Init()
        {
            if (ServerState >= ServerState.StartsUp)
                return;
            
            ServerState = ServerState.StartsUp;
            
            // TODO use dependency injection container 
            NetworkServer = Container.Resolve<BaseNetworkServer>();
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
            if (ServerState != ServerState.Running)
                return;
            
            ServerState = ServerState.Stopping;
            NetworkLoopCancellationToken.Cancel();
        }

        public override void ConfigureServices(Container services)
        {
            services.Register<BaseNetworkServer, NetworkServer>();
        }

        protected virtual void RunNetworkServer()
        {
            // TODO make bind address from config
            // ReSharper disable InconsistentNaming
            // var localEP = NetworkBase.TryGetLocalIP(out var localIP) 
            //     ? new IPEndPoint(localIP, 8303) 
            //     : new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8303);
            // ReSharper restore InconsistentNaming

            // ReSharper disable once InconsistentNaming
            var localEP = new IPEndPoint(IPAddress.Any, 8303);
            
            if (NetworkServer.Open(localEP))
            {
                NetworkLoopCancellationToken = new CancellationTokenSource();
                NetworkLoopThread = new Thread(RunNetworkLoop);
                NetworkLoopThread.Start(NetworkLoopCancellationToken.Token);
            }
            else
            {
                throw new Exception("Can't run network server");
            }
        }

        protected virtual void RunNetworkLoop(object obj)
        {
            var cancellationToken = (CancellationToken) obj;
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                if (NetworkServer.Receive())
                {
                    Console.WriteLine("READ");
                }
            }
        }

        protected virtual void NetworkUpdate()
        {
            NetworkServer.Update();
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

                NetworkUpdate();
                
                if (ServerState == ServerState.Stopping)
                    break;
            }
        }
        
        
        protected virtual void Update()
        {
            if (Tick % TickRate == 0)
                Console.WriteLine($"[{GameTimer.Elapsed:G}] Tick: {Tick}");
        }
    }
}
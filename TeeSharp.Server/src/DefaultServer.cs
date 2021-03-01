using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using Serilog;
using TeeSharp.Common.Config;
using TeeSharp.Common.Storage;
using TeeSharp.Network;
using TeeSharp.Core.Helpers;
using TeeSharp.Core.MinIoC;

namespace TeeSharp.Server
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class DefaultServer : BaseServer
    {
        public override int Tick { get; protected set; }

        protected new ServerConfiguration Config => (ServerConfiguration) base.Config;
        
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
            
            // TODO
            // Load serilog config from config.json:
            // LoggerConfiguration.ReadFrom.Configuration(configuration)
            
            const string consoleLogFormat = 
                "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}][{Level:u3}]{Message}{NewLine}{Exception}";
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: consoleLogFormat)
                .WriteTo.File(FSHelper.WorkingPath("log.txt"))
                .CreateLogger();
            
            Log.Information("[server] Start server initialization");
            
            Storage = Container.Resolve<BaseStorage>();
            Storage.Init(FSHelper.WorkingPath("storage.json"));
            
            base.Config = Container.Resolve<BaseConfiguration>();
            base.Config.Init();
            
            if (Storage.TryOpen("config.json", FileAccess.Read, out var fsConfig))
                Config.LoadConfig(fsConfig);
            
            Config.ServerName.OnChange += OnServerNameChanged;

            NetworkServer = Container.Resolve<BaseNetworkServer>();
            NetworkServer.Init();
            
            Log.Information($"Server name = {Config.ServerName}");
        }

        private void OnServerNameChanged(string serverName)
        {
            Log.Information($"[server] New server name: {serverName}");
        }

        public override void Run()
        {
            if (ServerState != ServerState.Running)
                ServerState = ServerState.Running;
            else
            {
                Log.Warning("[server] Server already in `Running` state");
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
            Log.CloseAndFlush();
        }

        public override void ConfigureServices(Container services)
        {
            services.Register<BaseChunkFactory, ChunkFactory>();
            services.Register<BaseStorage, Storage>().AsSingleton();
            services.Register<BaseConfiguration, ServerConfiguration>().AsSingleton();
            services.Register<BaseNetworkServer, NetworkServer>().AsSingleton();
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

                var responseToken = default(SecurityToken);
                if (NetworkServer.Receive(out var msg, ref responseToken))
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
        
        /// <summary>
        /// Game server tick
        /// </summary>
        protected virtual void Update()
        {
            if (Tick % TickRate == 0)
            {
                Log.Information($"[server] Tick: {Tick}");
                // Log.Information($"[server][{GameTimer.Elapsed:G}] Tick: {Tick}");
            }
        }
    }
}
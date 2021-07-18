using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Threading;
using Serilog;
using TeeSharp.Common.Commands;
using TeeSharp.Common.Config;
using TeeSharp.Common.Storage;
using TeeSharp.Network;
using TeeSharp.Core.Helpers;
using TeeSharp.Core.MinIoC;
using TeeSharp.MasterServer;

namespace TeeSharp.Server
{
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class DefaultServer : BaseServer
    {
        public override int Tick { get; protected set; }

        protected new ServerConfiguration Config { get; private set; }

        protected Stopwatch GameTimer { get; set; }

        protected const long TicksPerMillisecond = 10000;
        protected const long TicksPerSecond = TicksPerMillisecond * 1000;

        protected readonly TimeSpan TargetElapsedTime =
            TimeSpan.FromTicks(TicksPerSecond / TickRate);

        protected readonly TimeSpan MaxElapsedTime =
            TimeSpan.FromMilliseconds(500);

        protected TimeSpan AccumulatedElapsedTime;
        protected long PrevTicks = 0;

        protected readonly ConcurrentQueue<Tuple<NetworkMessage, SecurityToken>>
            NetworkMessagesQueue;

        public DefaultServer()
        {
            NetworkMessagesQueue = new ConcurrentQueue<Tuple<NetworkMessage, SecurityToken>>();
        }

        protected CancellationTokenSource NetworkLoopCancellationToken { get; set; }
        protected Thread NetworkLoopThread { get; set; }

        [SuppressMessage("ReSharper", "ArrangeThisQualifier")]
        public override void Init()
        {
            if (ServerState >= ServerState.StartsUp)
                return;

            ServerState = ServerState.StartsUp;

            // TODO
            // https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration-providers#json-configuration-provider
            // Load serilog config from appsettings:
            // LoggerConfiguration.ReadFrom.Configuration(configuration)

            const string consoleLogFormat =
                "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}][{Level:u3}]{Message}{NewLine}{Exception}";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: consoleLogFormat)
                .WriteTo.File(FileHelper.WorkingPath("log.txt"))
                .CreateLogger();

            Log.Information("[server] Initialization");

            Storage = Container.Resolve<BaseStorage>();
            Storage.Init(FileHelper.WorkingPath("storage.json"));

            base.Config = Container.Resolve<BaseConfiguration>();
            base.Config.Init();
            this.Config = (ServerConfiguration) base.Config;

            if (Storage.TryOpen("config.json", FileAccess.Read, out var fsConfig))
                Config.LoadConfig(fsConfig);

            Config.ServerName.OnChange += OnServerNameChanged;

            NetworkServer = Container.Resolve<BaseNetworkServer>();
            NetworkServer.Init(new NetworkServerConfig
            {
                MaxConnections = Config.MaxPlayers,
                MaxConnectionsPerIp = Config.MaxPlayersPerIp,
            });
        }

        protected virtual void OnServerNameChanged(string serverName)
        {
            Log.Information("[server] Server name changed to - {ServerName}", serverName);
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
            services.Register<BaseNetworkConnection, NetworkConnection>();
            services.Register<BaseCommandsDictionary, CommandsDictionary>();
            services.Register<BaseStorage, Storage>().AsSingleton();
            services.Register<BaseConfiguration, ServerConfiguration>().AsSingleton();
            services.Register<BaseNetworkServer, NetworkServer>().AsSingleton();
        }

        public override void SendServerInfo(ServerInfoType type, IPEndPoint addr, SecurityToken token)
        {
            throw new NotImplementedException();
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
            var localEP = new IPEndPoint(IPAddress.Any, Config.ServerPort);

            if (NetworkServer.Open(localEP))
            {
                NetworkLoopCancellationToken = new CancellationTokenSource();
                NetworkLoopThread = new Thread(RunNetworkLoop);
                NetworkLoopThread.Start(NetworkLoopCancellationToken.Token);

                Log.Information("[server] Local address - {Address}", NetworkServer.BindAddress.ToString());
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

                // TODO cancellationToken for NetworkServer.Receive

                var responseToken = default(SecurityToken);
                while (NetworkServer.Receive(out var msg, ref responseToken))
                {
                    NetworkMessagesQueue.Enqueue(
                        new Tuple<NetworkMessage, SecurityToken>(msg, responseToken)
                    );
                }
            }
        }

        protected override void ProcessNetworkMessage(NetworkMessage msg, SecurityToken responseToken)
        {
            if (msg.ClientId == -1)
                ProcessMasterServerMessage(msg, responseToken);
            else
                ProcessClientMessage(msg, responseToken);
        }

        protected override void ProcessMasterServerMessage(NetworkMessage msg,
            SecurityToken responseToken)
        {
            if (Packets.GetInfo.Length + 1 <= msg.Data.Length &&
                Packets.GetInfo.AsSpan()
                    .SequenceEqual(msg.Data.AsSpan(0, Packets.GetInfo.Length)))
            {
                if (msg.Flags.HasFlag(MessageFlags.Extended))
                {
                    // var extraToken = (SecurityToken) (((msg.ExtraData[0] << 8) | msg.ExtraData[1]) << 8);
                    // var token = msg.Data[Packets.GetInfo.Length] | extraToken;
                    // SendServerInfo(ServerInfoType.Extended, msg.EndPoint, token);

                    throw new NotImplementedException();
                }
                else
                {
                    if (responseToken != SecurityToken.Unknown && Config.UseSixUp)
                    {
                        throw new NotImplementedException();
                        // SendServerInfo(ServerInfoType.Vanilla, msg.EndPoint, token);
                    }
                }

                return;
            }

            if (Packets.GetInfo64Legacy.Length + 1 <= msg.Data.Length &&
                Packets.GetInfo64Legacy.AsSpan()
                    .SequenceEqual(msg.Data.AsSpan(0, Packets.GetInfo64Legacy.Length)))
            {
                var token = msg.Data[Packets.GetInfo.Length];
                SendServerInfo(ServerInfoType.Legacy64, msg.EndPoint, token);
            }
        }

        protected override void ProcessClientMessage(NetworkMessage msg,
            SecurityToken responseToken)
        {
        }

        protected virtual void NetworkUpdate()
        {
            while (NetworkMessagesQueue.TryDequeue(out var msgTuple))
                ProcessNetworkMessage(msgTuple.Item1, msgTuple.Item2);

            NetworkServer.Update();
        }

        protected virtual void RunLoop()
        {
            while (true)
            {
                BeginLoop:

                var currentTicks = GameTimer.Elapsed.Ticks;
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
                Log.Information("[server] Tick - {Tick}", Tick);
            }
        }
    }
}
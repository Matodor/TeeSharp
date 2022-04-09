// using System;
// using System.Collections.Concurrent;
// using System.Diagnostics;
// using System.Diagnostics.CodeAnalysis;
// using System.IO;
// using System.Net;
// using System.Threading;
// using Serilog;
// using TeeSharp.Commands;
// using TeeSharp.Common.Config;
// using TeeSharp.Common.Storage;
// using TeeSharp.Network;
// using TeeSharp.Core.Helpers;
// using TeeSharp.MasterServer;
//
// namespace TeeSharp.Server;
//
// public class B1asicGameServer : IGameServer
// {
//
//
//     protected readonly ConcurrentQueue<Tuple<NetworkMessage, SecurityToken>>
//         NetworkMessagesQueue;
//
//     public BasicGameServer()
//     {
//         NetworkMessagesQueue = new ConcurrentQueue<Tuple<NetworkMessage, SecurityToken>>();
//     }
//
//     protected CancellationTokenSource NetworkLoopCancellationToken { get; set; }
//     protected Thread NetworkLoopThread { get; set; }
//
//     [SuppressMessage("ReSharper", "ArrangeThisQualifier")]
//     public override void Init()
//     {
//         Storage = Container.Resolve<BaseStorage>();
//         Storage.Init(FileHelper.WorkingPath("storage.json"));
//
//         base.Config = Container.Resolve<BaseConfiguration>();
//         base.Config.Init();
//         this.Config = (ServerConfiguration) base.Config;
//
//         if (Storage.TryOpen("config.json", FileAccess.Read, out var fsConfig))
//             Config.LoadConfig(fsConfig);
//
//         Config.ServerName.OnChange += OnServerNameChanged;
//
//         NetworkServer = Container.Resolve<BaseNetworkServer>();
//         NetworkServer.Init(new NetworkServerConfig
//         {
//             MaxConnections = Config.MaxPlayers,
//             MaxConnectionsPerIp = Config.MaxPlayersPerIp,
//         });
//     }
//
//     protected virtual void OnServerNameChanged(string serverName)
//     {
//         Log.Information("Server name changed to - {ServerName}", serverName);
//     }
//
//     public override void Run()
//     {
//         if (ServerState != ServerState.Running)
//             ServerState = ServerState.Running;
//         else
//         {
//             Log.Warning("Server already in `Running` state");
//             return;
//         }
//
//         GameTimer = Stopwatch.StartNew();
//         Tick = 0;
//
//         RunNetworkServer();
//         RunLoop();
//     }
//
//     public override void Stop()
//     {
//         if (ServerState != ServerState.Running)
//             return;
//
//         ServerState = ServerState.Stopping;
//         NetworkLoopCancellationToken.Cancel();
//     }
//
//     public override void SendServerInfo(ServerInfoType type, IPEndPoint addr, SecurityToken token)
//     {
//         throw new NotImplementedException();
//     }
//
//     protected virtual void RunNetworkServer()
//     {
//         // TODO make bind address from config
//         // ReSharper disable InconsistentNaming
//         // var localEP = NetworkBase.TryGetLocalIP(out var localIP)
//         //     ? new IPEndPoint(localIP, 8303)
//         //     : new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8303);
//         // ReSharper restore InconsistentNaming
//
//         // ReSharper disable once InconsistentNaming
//         var localEP = new IPEndPoint(IPAddress.Any, Config.ServerPort);
//
//         if (NetworkServer.Open(localEP))
//         {
//             NetworkLoopCancellationToken = new CancellationTokenSource();
//             NetworkLoopThread = new Thread(RunNetworkLoop);
//             NetworkLoopThread.Start(NetworkLoopCancellationToken.Token);
//
//             Log.Information("Local address - {Address}", NetworkServer.BindAddress.ToString());
//         }
//         else
//         {
//             throw new Exception("Can't run network server");
//         }
//     }
//
//     protected virtual void RunNetworkLoop(object obj)
//     {
//         var cancellationToken = (CancellationToken) obj;
//         while (true)
//         {
//             if (cancellationToken.IsCancellationRequested)
//                 break;
//
//             // TODO cancellationToken for NetworkServer.Receive
//
//             var responseToken = default(SecurityToken);
//             while (NetworkServer.Receive(out var msg, ref responseToken))
//             {
//                 NetworkMessagesQueue.Enqueue(
//                     new Tuple<NetworkMessage, SecurityToken>(msg, responseToken)
//                 );
//             }
//         }
//     }
//
//     protected override void ProcessNetworkMessage(NetworkMessage msg, SecurityToken responseToken)
//     {
//         if (msg.ClientId == -1)
//             ProcessMasterServerMessage(msg, responseToken);
//         else
//             ProcessClientMessage(msg, responseToken);
//     }
//
//     protected override void ProcessMasterServerMessage(NetworkMessage msg,
//         SecurityToken responseToken)
//     {
//         if (Packets.GetInfo.Length + 1 <= msg.Data.Length &&
//             Packets.GetInfo.AsSpan()
//                 .SequenceEqual(msg.Data.AsSpan(0, Packets.GetInfo.Length)))
//         {
//             if (msg.Flags.HasFlag(MessageFlags.Extended))
//             {
//                 // var extraToken = (SecurityToken) (((msg.ExtraData[0] << 8) | msg.ExtraData[1]) << 8);
//                 // var token = msg.Data[Packets.GetInfo.Length] | extraToken;
//                 // SendServerInfo(ServerInfoType.Extended, msg.EndPoint, token);
//
//                 throw new NotImplementedException();
//             }
//             else
//             {
//                 if (responseToken != SecurityToken.Unknown && Config.UseSixUp)
//                 {
//                     throw new NotImplementedException();
//                     // SendServerInfo(ServerInfoType.Vanilla, msg.EndPoint, token);
//                 }
//             }
//
//             return;
//         }
//
//         if (Packets.GetInfo64Legacy.Length + 1 <= msg.Data.Length &&
//             Packets.GetInfo64Legacy.AsSpan()
//                 .SequenceEqual(msg.Data.AsSpan(0, Packets.GetInfo64Legacy.Length)))
//         {
//             var token = msg.Data[Packets.GetInfo.Length];
//             SendServerInfo(ServerInfoType.Legacy64, msg.EndPoint, token);
//         }
//     }
//
//     protected override void ProcessClientMessage(NetworkMessage msg,
//         SecurityToken responseToken)
//     {
//     }
//
//     protected virtual void NetworkUpdate()
//     {
//         while (NetworkMessagesQueue.TryDequeue(out var msgTuple))
//             ProcessNetworkMessage(msgTuple.Item1, msgTuple.Item2);
//
//         NetworkServer.Update();
//     }
// }
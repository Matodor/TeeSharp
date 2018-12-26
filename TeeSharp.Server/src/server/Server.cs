using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Game;
using TeeSharp.Common.Protocol;
using TeeSharp.Common.Snapshots;
using TeeSharp.Common.Storage;
using TeeSharp.Core;
using TeeSharp.Map;
using TeeSharp.MasterServer;
using TeeSharp.Network;
using TeeSharp.Network.Enums;
using TeeSharp.Server.Game;
using Debug = TeeSharp.Core.Debug;

namespace TeeSharp.Server
{
    public class ServerKernelConfig : DefaultKernelConfig
    {
        public override void Load(IKernel kernel)
        {
            base.Load(kernel);

            kernel.Bind<BaseServer>().To<Server>().AsSingleton();
            kernel.Bind<BaseConfig>().To<ServerConfig>().AsSingleton();
            kernel.Bind<BaseGameContext>().To<GameContext>().AsSingleton();
            kernel.Bind<BaseNetworkServer>().To<NetworkServer>().AsSingleton();
            kernel.Bind<BaseRegister>().To<Register>().AsSingleton();
            kernel.Bind<BaseNetworkBan>().To<NetworkBan>().AsSingleton();
            kernel.Bind<BaseVotes>().To<Votes>().AsSingleton();
            kernel.Bind<BaseEvents>().To<Events>().AsSingleton();
            kernel.Bind<BaseMapCollision>().To<MapCollision>().AsSingleton();
            kernel.Bind<BaseTuningParams>().To<TuningParams>().AsSingleton();
            kernel.Bind<BaseGameWorld>().To<GameWorld>().AsSingleton();

            kernel.Bind<BaseServerClient>().To<ServerClient>();
            kernel.Bind<BasePlayer>().To<Player>();

            // singletons
            //kernel.Bind<BaseStorage>().To<Storage>().AsSingleton();
        }
    }

    public class Server : BaseServer
    {
        public override int MaxClients => 64;
        public override int MaxPlayers => 16;
        public override int TickSpeed => 50;

        public override void Init(string[] args)
        {
            Tick = 0;
            StartTime = 0;

            SnapshotIdPool = new SnapshotIdPool();
            SnapshotBuilder = new SnapshotBuilder();

            Config = Kernel.Get<BaseConfig>();
            GameContext = Kernel.Get<BaseGameContext>();
            Storage = Kernel.Get<BaseStorage>();
            NetworkServer = Kernel.Get<BaseNetworkServer>();
            Console = Kernel.Get<BaseGameConsole>();
            Register = Kernel.Get<BaseRegister>();
            NetworkBan = Kernel.Get<BaseNetworkBan>();

            if (Config == null ||
                GameContext == null ||
                Storage == null ||
                NetworkServer == null ||
                Console == null)
            {
                throw new Exception("Register components fail");
            }

            Clients = new BaseServerClient[MaxClients];
            for (var i = 0; i < Clients.Length; i++)
                Clients[i] = Kernel.Get<BaseServerClient>();

            Storage.Init("TeeSharp", StorageType.Server);
            Console.Init();
            Console.RegisterPrintCallback((OutputLevel) Config["ConsoleOutputLevel"].AsInt(), SendRconLineAuthed);
            NetworkServer.Init();

            RegisterConsoleCommands();
            GameContext.RegisterConsoleCommands();

            Console.ExecuteFile("autoexec.cfg");
            Console.ParseArguments(args);
        }

        public override void Run()
        {
            if (IsRunning)
                return;

            Debug.Log("server", "starting...");

            if (!LoadMap(Config["SvMap"]))
            {
                Debug.Error("server", $"failed to load map. mapname='{Config["SvMap"]}'");
                return;    
            }

            if (!StartNetworkServer())
                return;

            Console.Print(OutputLevel.Standard, "server", $"server name is '{Config["SvName"]}'");
            GameContext.OnInit();

            StartTime = Time.Get();
            IsRunning = true;

            while (IsRunning)
            {
                var now = Time.Get();
                var ticks = 0;

                while (now > TickStartTime(Tick + 1))
                {
                    Tick++;
                    ticks++;

                    for (var clientId = 0; clientId < Clients.Length; clientId++)
                    {
                        if (Clients[clientId].State != ServerClientState.InGame)
                            continue;

                        for (var inputIndex = 0; inputIndex < Clients[clientId].Inputs.Length; inputIndex++)
                        {
                            if (Clients[clientId].Inputs[inputIndex].Tick != Tick)
                                continue;

                            GameContext.OnClientPredictedInput(clientId, Clients[clientId].Inputs[inputIndex].Data);
                            break;
                        }
                    }

                    GameContext.OnTick();
                }

                if (ticks != 0)
                {
                    if (Tick % 2 == 0 || Config["SvHighBandwidth"])
                        DoSnapshot();
                    // UpdateClientRconCommands()
                }

                //Register.RegisterUpdate(NetworkServer.NetType());
                PumpNetwork();

                Thread.Sleep(5);
            }

            for (var i = 0; i < Clients.Length; i++)
            {
                if (Clients[i].State != ServerClientState.Empty)
                    NetworkServer.Drop(i, "Server shutdown");
            }

            GameContext.OnShutdown();
        }
        
        public override string ClientName(int clientId)
        {
            return Clients[clientId].State == ServerClientState.InGame
                ? Clients[clientId].Name
                : string.Empty;
        }

        public override void ClientName(int clientId, string name)
        {
            if (string.IsNullOrEmpty(name) || Clients[clientId].State < ServerClientState.Ready)
                return;

            Clients[clientId].Name = name.Limit(BaseServerClient.MaxNameLength);
        }

        public override string ClientClan(int clientId)
        {
            return Clients[clientId].State == ServerClientState.InGame
                ? Clients[clientId].Clan
                : string.Empty;
        }

        public override void ClientClan(int clientId, string clan)
        {
            if (string.IsNullOrEmpty(clan) || Clients[clientId].State < ServerClientState.Ready)
                return;

            Clients[clientId].Clan = clan.Limit(BaseServerClient.MaxNameLength);
        }

        public override int ClientCountry(int clientId)
        {
            return Clients[clientId].State == ServerClientState.InGame 
                ? Clients[clientId].Country 
                : -1;
        }

        public override void ClientCountry(int clientId, int country)
        {
            if (Clients[clientId].State < ServerClientState.Ready)
                return;

            Clients[clientId].Country = country;
        }

        protected override void GenerateRconPassword()
        {
            throw new NotImplementedException();
        }

        public override IPEndPoint ClientEndPoint(int clientId)
        {
            return Clients[clientId].State == ServerClientState.InGame
                ? NetworkServer.ClientEndPoint(clientId)
                : null;
        }

        public override ClientInfo ClientInfo(int clientId)
        {
            if (Clients[clientId].State != ServerClientState.InGame)
                return null;

            return new ClientInfo
            {
                Name = ClientName(clientId),
                Latency = Clients[clientId].Latency,
            };
        }

        public override bool ClientInGame(int clientId)
        {
            return Clients[clientId].State == ServerClientState.InGame;
        }

        public override bool IsAuthed(int clientId)
        {
            // TODO
            return false;
        }

        public override bool SendMsg(MsgPacker msg, MsgFlags flags, int clientId)
        {
            if (msg == null)
                return false;

            var packet = new Chunk()
            {
                ClientId = clientId,
                DataSize = msg.Size(),
                Data = msg.Data(),
                Flags = SendFlags.None,
            };

            if (flags.HasFlag(MsgFlags.Vital))
                packet.Flags |= SendFlags.Vital;
            if (flags.HasFlag(MsgFlags.Flush))
                packet.Flags |= SendFlags.Flush;


            if (!flags.HasFlag(MsgFlags.NoRecord))
            {
                // TODO demo record
            }

            if (flags.HasFlag(MsgFlags.NoSend))
                return true;

            if (clientId == -1)
            {
                for (var i = 0; i < Clients.Length; i++)
                {
                    if (Clients[i].State == ServerClientState.InGame &&
                        Clients[i].Quitting == false)
                    {
                        packet.ClientId = i;
                        NetworkServer.Send(packet);
                    }
                }
            }
            else
            {
                NetworkServer.Send(packet);
            }

            return true;
        }

        //public override bool SendMsgEx(MsgPacker msg, MsgFlags flags, int clientId, bool system)
        //{
        //    if (msg == null)
        //        return false;

        //    var packet = new Chunk()
        //    {
        //        ClientId = clientId,
        //        DataSize = msg.Size(),
        //        Data = msg.Data(),
        //    };

        //    packet.Data[0] <<= 1;
        //    if (system)
        //        packet.Data[0] |= 1;

        //    if (flags.HasFlag(MsgFlags.Vital))
        //        packet.Flags |= SendFlags.VITAL;
        //    if (flags.HasFlag(MsgFlags.Flush))
        //        packet.Flags |= SendFlags.FLUSH;

        //    if (!flags.HasFlag(MsgFlags.NoSend))
        //    {
        //        if (clientId == -1)
        //        {
        //            for (var i = 0; i < Clients.Length; i++)
        //            {
        //                if (Clients[i].State != ServerClientState.IN_GAME)
        //                    continue;

        //                packet.ClientId = i;
        //                NetworkServer.Send(packet);
        //            }
        //        }
        //        else
        //        {
        //            NetworkServer.Send(packet);
        //        }
        //    }

        //    return true;
        //}

        public override bool SendPackMsg<T>(T msg, MsgFlags flags, int clientId)
        {
            var packer = new MsgPacker((int) msg.Type, false);
            if (msg.PackError(packer))
                return false;
            return SendMsg(packer, flags, clientId);
        }

        public override bool SnapshotItem<T>(T item, int id)
        {
            Debug.Assert(id >= 0 && id <= 65535, "incorrect id");
            return id >= 0 && SnapshotBuilder.AddItem(item, id);
        }

        public override T SnapshotItem<T>(int id)
        {
            Debug.Assert(id >= 0 && id <= 65535, "incorrect id");

            return id < 0
                ? null
                : SnapshotBuilder.NewItem<T>(id);
        }

        public override int SnapshotNewId()
        {
            return SnapshotIdPool.NewId();
        }

        public override void SnapshotFreeId(int id)
        {
            SnapshotIdPool.FreeId(id);
        }

        protected override bool StartNetworkServer()
        {
            var bindAddr = NetworkHelper.GetLocalIP(AddressFamily.InterNetwork);
            if (!string.IsNullOrWhiteSpace(Config["Bindaddr"]))
                bindAddr = IPAddress.Parse(Config["Bindaddr"]);

            var networkConfig = new NetworkServerConfig
            {
                BindEndPoint = new IPEndPoint(bindAddr, Config["SvPort"]),
                MaxClientsPerIp = Math.Clamp(Config["SvMaxClientsPerIP"], 1, MaxClients),
                MaxClients = Math.Clamp(Config["SvMaxClients"], 1, MaxClients),
                ConnectionConfig = new ConnectionConfig
                {
                    Timeout = Config["ConnTimeout"]
                }
            };

            if (!NetworkServer.Open(networkConfig))
            {
                Debug.Error("server", $"couldn't open socket. port {networkConfig.BindEndPoint.Port} might already be in use");
                return false;
            }

            NetworkServer.SetCallbacks(ClientConnected, ClientDisconnected);
            Debug.Log("server", $"network server running at: {networkConfig.BindEndPoint}");
            return true;
        }

        protected override void ProcessClientPacket(Chunk packet)
        {
            var clientId = packet.ClientId;
            var unpacker = new UnPacker();
            unpacker.Reset(packet.Data, packet.DataSize);

            var msg = unpacker.GetInt();
            var system = (msg & 1) != 0;
            msg >>= 1;

            if (unpacker.Error)
                return;

            if (system)
            {
                var networkMsg = (NetworkMessages) msg;
                switch (networkMsg)
                {
                    case NetworkMessages.ClientInfo:
                        NetMsgInfo(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.ClientRequestMapData:
                        NetMsgRequestMapData(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.ClientReady:
                        NetMsgReady(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.ClientEnterGame:
                        NetMsgEnterGame(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.ClientInput:
                        NetMsgInput(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.ClientRconCommand:
                        NetMsgRconCmd(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.ClientRconAuth:
                        NetMsgRconAuth(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.Ping:
                        NetMsgPing(packet, unpacker, clientId);
                        break;
                    default:
                        Console.Print(OutputLevel.Debug, "server", $"strange message clientId={clientId} msg={msg} data_size={packet.DataSize}");
                        break;
                }
            }
            else if (packet.Flags.HasFlag(SendFlags.Vital) && 
                     Clients[clientId].State >= ServerClientState.Ready)
            {
                GameContext.OnMessage((GameMessage) msg, unpacker, clientId);
            }
        }

        protected override void GenerateServerInfo(Packer packer, int token)
        {
            var playersCount = 0;
            var clientsCount = 0;

            for (var i = 0; i < MaxClients; i++)
            {
                if (Clients[i].State == ServerClientState.Empty)
                    continue;

                if (GameContext.IsClientPlayer(i))
                    playersCount++;
                clientsCount++;
            }

            if (token != -1)
            {
                packer.Reset();
                packer.AddRaw(MasterServerPackets.Info);
                packer.AddInt(token);
            }

            packer.AddString(GameContext.GameVersion, 32);
            packer.AddString(Config["SvName"], 64);
            packer.AddString(Config["SvHostname"], 128);
            packer.AddString(CurrentMap.MapName, 32);
            packer.AddString(GameContext.GameController.GameType, 16);

            var flag = string.IsNullOrEmpty(Config["Password"]) ? 0 : ServerInfoFlagPassword;
            packer.AddInt(flag);
            packer.AddInt(Config["SvSkillLevel"]);
            packer.AddInt(playersCount);
            packer.AddInt(Config["SvPlayerSlots"]);
            packer.AddInt(clientsCount);
            packer.AddInt(MaxClients);

            if (token != -1)
            {
                for (var i = 0; i < MaxClients; i++)
                {
                    if (Clients[i].State != ServerClientState.Empty)
                    {
                        packer.AddString(ClientName(i), BaseServerClient.MaxNameLength);
                        packer.AddString(ClientClan(i), BaseServerClient.MaxClanLength);
                        packer.AddInt(ClientCountry(i));
                        packer.AddInt(GameContext.GameController.Score(i)); // TODO client score
                        packer.AddInt(GameContext.IsClientPlayer(i) ? 0 : 1); // flag spectator=1, bot=2 (player=0)
                    }
                }
            }
        }

        protected override void SendServerInfo(int clientId)
        {
            var msg = new MsgPacker((int) NetworkMessages.ServerInfo, true);
            GenerateServerInfo(msg, -1);

            if (clientId == -1)
            {
                for (var i = 0; i < Clients.Length; i++)
                {
                    if (Clients[i].State != ServerClientState.Empty)
                        SendMsg(msg, MsgFlags.Vital | MsgFlags.Flush, i);
                }
            }
            else if (Clients[clientId].State != ServerClientState.Empty)
                SendMsg(msg, MsgFlags.Vital | MsgFlags.Flush, clientId);
        }

        protected override void NetMsgPing(Chunk packet, UnPacker unPacker, int clientId)
        {
            var msg = new MsgPacker((int) NetworkMessages.PingReply, true);
            SendMsg(msg, MsgFlags.None, clientId);
        }

        protected override void NetMsgRconAuth(Chunk packet, UnPacker unPacker, int clientId)
        {
            var password = unPacker.GetString(SanitizeType.SanitizeCC);

            if (!packet.Flags.HasFlag(SendFlags.Vital) || unPacker.Error)
                return;

            // TODO
            //var login = unPacker.GetString();
            //var password = unPacker.GetString();

            //SendRconLine(clientId, password);
            //if (!packet.Flags.HasFlag(SendFlags.VITAL) || unPacker.Error)
            //    return;

            //if (string.IsNullOrEmpty(Config["SvRconPassword"]) &&
            //    string.IsNullOrEmpty(Config["SvRconModPassword"]))
            //{
            //    SendRconLine(clientId, "No rcon password set on server. Set sv_rcon_password and/or sv_rcon_mod_password to enable the remote console.");
            //}

            //var authed = false;
            //if (password == Config["SvRconPassword"])
            //{
            //    authed = true;
            //}
            //else if (password == Config["SvRconModPassword"])
            //{
            //    authed = true;
            //}

            //if (authed)
            //{
            //    var msg = new MsgPacker((int) NetworkMessages.SV_RCON_AUTH_STATUS);
            //    msg.AddInt(1);
            //    msg.AddInt(1);
            //    SendMsgEx(msg, MsgFlags.Vital, clientId, true);
            //}
        }

        protected override void NetMsgRconCmd(Chunk packet, UnPacker unPacker, int clientId)
        {
            if (!packet.Flags.HasFlag(SendFlags.Vital) || unPacker.Error)
                return;

            // TODO
        }

        protected override void NetMsgInput(Chunk packet, UnPacker unPacker, int clientId)
        {
            Clients[clientId].LastAckedSnapshot = unPacker.GetInt();

            var intendedTick = unPacker.GetInt();
            var size = unPacker.GetInt();
            var now = Time.Get();

            if (unPacker.Error || size / sizeof(int) > BaseServerClient.MaxInputSize)
                return;

            if (Clients[clientId].LastAckedSnapshot > 0)
                Clients[clientId].SnapshotRate = SnapshotRate.Full;

            if (intendedTick > Clients[clientId].LastInputTick)
            {
                var timeLeft = ((TickStartTime(intendedTick) - now) * 1000) / Time.Freq();
                var msg = new MsgPacker((int) NetworkMessages.ServerInputTiming, true);
                msg.AddInt(intendedTick);
                msg.AddInt((int) timeLeft);
                SendMsg(msg, MsgFlags.None, clientId);
            }

            Clients[clientId].LastInputTick = intendedTick;

            if (intendedTick <= Tick)
                intendedTick = Tick + 1;

            var input = Clients[clientId].Inputs[Clients[clientId].CurrentInput];
            input.Tick = intendedTick;

            for (var i = 0; i < size / sizeof(int); i++)
                input.Data[i] = unPacker.GetInt();

            var pingCorrection = Math.Clamp(unPacker.GetInt(), 0, 50);

            if (Clients[clientId].SnapshotStorage.Get(Clients[clientId].LastAckedSnapshot,
                out var tagTime, out _))
            {
                Clients[clientId].Latency = (int) (((now - tagTime) * 1000) / Time.Freq());
                Clients[clientId].Latency = Math.Max(0, Clients[clientId].Latency - pingCorrection);
            }

            Array.Copy(input.Data, 0, Clients[clientId].LatestInput.Data, 0, BaseServerClient.MaxInputSize);

            Clients[clientId].CurrentInput++;
            Clients[clientId].CurrentInput &= BaseServerClient.MaxInputs;

            if (Clients[clientId].State == ServerClientState.InGame)
                GameContext.OnClientDirectInput(clientId, Clients[clientId].LatestInput.Data);
        }

        protected override void NetMsgEnterGame(Chunk packet, UnPacker unPacker, int clientId)
        {
            if (!packet.Flags.HasFlag(SendFlags.Vital))
                return;

            if (Clients[clientId].State != ServerClientState.Ready)
                return;

            if (!GameContext.IsClientReady(clientId))
                return;

            Console.Print(OutputLevel.Standard, "server", $"player has entered the game. ClientID={clientId} addr={NetworkServer.ClientEndPoint(clientId)}");
            Clients[clientId].State = ServerClientState.InGame;
            SendServerInfo(clientId);
            GameContext.OnClientEnter(clientId);
        }

        protected override void NetMsgReady(Chunk packet, UnPacker unPacker, int clientId)
        {
            if (!packet.Flags.HasFlag(SendFlags.Vital))
                return;

            if (Clients[clientId].State != ServerClientState.Connecting)
                return;

            Console.Print(OutputLevel.AddInfo, "server", $"player is ready. ClientID={clientId} addr={NetworkServer.ClientEndPoint(clientId)}");
            Clients[clientId].State = ServerClientState.Ready;
            GameContext.OnClientConnected(clientId);

            var msg = new MsgPacker((int) NetworkMessages.ServerConnectionReady, true);
            SendMsg(msg, MsgFlags.Vital | MsgFlags.Flush, clientId);
        }

        protected override void NetMsgRequestMapData(Chunk packet, UnPacker unPacker, int clientId)
        {
            if (packet.Flags.HasFlag(SendFlags.Vital) &&
                Clients[clientId].State != ServerClientState.Connecting)
            {
                return;
            }

            var chunkSize = MapChunkSize;
            for (var i = 0; i < Config["SvMapDownloadSpeed"] && Clients[clientId].MapChunk >= 0; i++)
            {
                var chunk = Clients[clientId].MapChunk;
                var offset = chunk * chunkSize;

                if (offset + chunkSize >= CurrentMap.Size)
                {
                    chunkSize = CurrentMap.Size - offset;
                    Clients[clientId].MapChunk = -1;
                }
                else
                {
                    Clients[clientId].MapChunk++;
                }

                var msg = new MsgPacker((int) NetworkMessages.ServerMapData, true);
                msg.AddRaw(CurrentMap.RawData, offset, chunkSize);
                SendMsg(msg, MsgFlags.Vital | MsgFlags.Flush, clientId);

                Debug.Log("server", $"sending chunk {chunk} with size {chunkSize}");
            }
        }

        protected override void NetMsgInfo(Chunk packet, UnPacker unPacker, int clientId)
        {
            if (!packet.Flags.HasFlag(SendFlags.Vital))
                return;

            if (Clients[clientId].State != ServerClientState.Auth)
                return;

            var version = unPacker.GetString(SanitizeType.SanitizeCC);
            if (string.IsNullOrEmpty(version) || !version.StartsWith(GameContext.NetVersion))
            {
                NetworkServer.Drop(clientId, $"Wrong version. Server is running '{GameContext.NetVersion}' and client '{version}'");
                return;
            }

            var password = unPacker.GetString(SanitizeType.SanitizeCC);
            if (!string.IsNullOrEmpty(Config["Password"]) && password != Config["Password"])
            {
                NetworkServer.Drop(clientId, "Wrong password");
                return;
            }

            Clients[clientId].Version = unPacker.GetInt();
            Clients[clientId].State = ServerClientState.Connecting;
            SendMap(clientId);
        }

        protected override void PumpNetwork()
        {
            NetworkServer.Update();

            Chunk packet = null;
            uint responseToken = 0;

            while (NetworkServer.Receive(ref packet, ref responseToken))
            {
                if (packet.Flags.HasFlag(SendFlags.Connless))
                {
                    if (Register.RegisterProcessPacket(packet, responseToken))
                        continue;

                    if (packet.DataSize >= MasterServerPackets.GetInfo.Length &&
                        packet.Data.ArrayCompare(MasterServerPackets.GetInfo, MasterServerPackets.GetInfo.Length))
                    {
                        var unpacker = new UnPacker();
                        unpacker.Reset(packet.Data, packet.DataSize, MasterServerPackets.GetInfo.Length);
                        var serverBrowserToken = unpacker.GetInt();

                        if (unpacker.Error)
                            continue;
                        
                        var packer = new Packer();
                        GenerateServerInfo(packer, serverBrowserToken);

                        var response = new Chunk()
                        {
                            ClientId = -1,
                            EndPoint = packet.EndPoint,
                            Flags = SendFlags.Connless,
                            Data = packer.Data(),
                            DataSize = packer.Size(),
                        };

                        NetworkServer.Send(response, responseToken);
                    }
                }
                else
                {
                    ProcessClientPacket(packet);
                }
            }

            // TODO Econ.Update(); 
            NetworkBan.Update();
        }

        protected override void DoSnapshot()
        {
            GameContext.OnBeforeSnapshot();
            // TODO demo recorder

            for (var i = 0; i < Clients.Length; i++)
            {
                if (Clients[i].State != ServerClientState.InGame)
                    continue;

                if (Clients[i].SnapshotRate == SnapshotRate.Recover && Tick % TickSpeed != 0)
                    continue;
                
                if (Clients[i].SnapshotRate == SnapshotRate.Init && Tick % 10 != 0)
                    continue;

                SnapshotBuilder.Start();
                GameContext.OnSnapshot(i);

                var now = Time.Get();
                var snapshot =  SnapshotBuilder.Finish();
                var crc = snapshot.Crc();

                Clients[i].SnapshotStorage.PurgeUntil(Tick - TickSpeed * 3);
                Clients[i].SnapshotStorage.Add(Tick, now, snapshot);

                var deltaTick = -1;

                if (Clients[i].SnapshotStorage.Get(Clients[i].LastAckedSnapshot,
                    out _, out var deltaSnapshot))
                {
                    deltaTick = Clients[i].LastAckedSnapshot;
                }
                else
                {
                    deltaSnapshot = new Snapshot(new SnapshotItem[0], 0);
                    if (Clients[i].SnapshotRate == SnapshotRate.Full)
                        Clients[i].SnapshotRate = SnapshotRate.Recover;
                }

                var deltaData = new int[Snapshot.MaxSize / sizeof(int)];
                var deltaSize = SnapshotDelta.CreateDelta(deltaSnapshot, snapshot, deltaData);

                if (deltaSize == 0)
                {
                    var msg = new MsgPacker((int) NetworkMessages.ServerSnapEmpty, true);
                    msg.AddInt(Tick);
                    msg.AddInt(Tick - deltaTick);
                    SendMsg(msg, MsgFlags.Flush, i);
                    continue;
                }

                var snapData = new byte[Snapshot.MaxSize];
                var snapshotSize = IntCompression.Compress(deltaData, 0, deltaSize, snapData, 0);
                var numPackets = (snapshotSize + Snapshot.MaxPacketSize - 1) / Snapshot.MaxPacketSize;

                for (int n = 0, left = snapshotSize; left != 0; n++)
                {
                    var chunk = left < Snapshot.MaxPacketSize ? left : Snapshot.MaxPacketSize;
                    left -= chunk;

                    if (numPackets == 1)
                    {
                        var msg = new MsgPacker((int) NetworkMessages.ServerSnapSingle, true);
                        msg.AddInt(Tick);
                        msg.AddInt(Tick - deltaTick);
                        msg.AddInt(crc);
                        msg.AddInt(chunk);
                        msg.AddRaw(snapData, n * Snapshot.MaxPacketSize, chunk);
                        SendMsg(msg, MsgFlags.Flush, i);
                    }
                    else
                    {
                        var msg = new MsgPacker((int) NetworkMessages.ServerSnap, true);
                        msg.AddInt(Tick);
                        msg.AddInt(Tick - deltaTick);
                        msg.AddInt(numPackets);
                        msg.AddInt(n);
                        msg.AddInt(crc);
                        msg.AddInt(chunk);
                        msg.AddRaw(snapData, n * Snapshot.MaxPacketSize, chunk);
                        SendMsg(msg, MsgFlags.Flush, i);
                    }
                }
            }

            GameContext.OnAfterSnapshots();
        }

        protected override long TickStartTime(int tick)
        {
            return StartTime + (Time.Freq() * tick) / TickSpeed;
        }

        protected override void ClientDisconnected(int clientId, string reason)
        {
            Console.Print(OutputLevel.Standard, "server", 
                $"client disconnected. cid={clientId} addr={NetworkServer.ClientEndPoint(clientId)} reason='{reason}'");

            if (Clients[clientId].State >= ServerClientState.Ready)
            {
                Clients[clientId].Quitting = true;
                GameContext.OnClientDisconnect(clientId, reason);
            }

            Clients[clientId].State = ServerClientState.Empty;
        }

        protected override void ClientConnected(int clientid)
        {
            Clients[clientid].State = ServerClientState.Auth;
            Clients[clientid].Name = null;
            Clients[clientid].Clan = null;
            Clients[clientid].Country = -1;
            Clients[clientid].Quitting = false;
            Clients[clientid].Reset();
        }

        protected override bool LoadMap(string mapName)
        {
            mapName = Path.GetFileNameWithoutExtension(mapName);
            var path = $"maps/{mapName}.map";

            Console.Print(OutputLevel.Debug, "map", $"loading map='{path}'");

            using (var stream = Storage.OpenFile(path, FileAccess.Read))
            {
                if (stream == null)
                {
                    Console.Print(OutputLevel.Debug, "map", $"could not open map='{path}'");
                    return false;
                }

                CurrentMap = MapContainer.Load(stream, out var error);
                if (CurrentMap == null)
                {
                    Console.Print(OutputLevel.Debug, "map", $"error with load map='{path}' ({error})");
                    return false;
                }
                CurrentMap.MapName = mapName;
                Console.Print(OutputLevel.Debug, "map", $"successful load map='{path}' ({error})");

                return true;
            }
        }

        protected override void SendMap(int clientId)
        {
            var msg = new MsgPacker((int) NetworkMessages.ServerMapChange, true);
            // map name
            msg.AddString(CurrentMap.MapName);
            // map crc
            msg.AddInt((int) CurrentMap.CRC);
            // map size
            msg.AddInt(CurrentMap.Size);
            // map chunks per request
            msg.AddInt(Config["SvMapDownloadSpeed"]);
            // map chunk size
            msg.AddInt(MapChunkSize);

            SendMsg(msg, MsgFlags.Vital | MsgFlags.Flush, clientId);
        }

        protected override void SendRconLine(int clientId, string line)
        {
            var packer = new MsgPacker((int) NetworkMessages.ServerRconLine, true);
            packer.AddString(line, 512);
            SendMsg(packer, MsgFlags.Vital, clientId);
        }

        protected override void SendRconLineAuthed(string message, object data)
        {
            // TODO
        }

        protected override void SendRconCommandAdd(ConsoleCommand command, int clientId)
        {
            var msg = new MsgPacker((int) NetworkMessages.ServerRconCommandAdd, true);
            msg.AddString(command.Cmd, ConsoleCommand.MaxCmdLength);
            msg.AddString(command.Description, ConsoleCommand.MaxDescLength);
            msg.AddString(command.Format, ConsoleCommand.MaxParamsLength);
            SendMsg(msg, MsgFlags.Vital, clientId);
        }

        protected override void SendRconCommandRem(ConsoleCommand command, int clientId)
        {
            var msg = new MsgPacker((int) NetworkMessages.ServerRconCommandRemove, true);
            msg.AddString(command.Cmd, ConsoleCommand.MaxCmdLength);
            SendMsg(msg, MsgFlags.Vital, clientId);
        }

        protected override void RegisterConsoleCommands()
        {
            Console.RegisterCommand("kick", "i?s", ConsoleKick, ConfigFlags.SERVER, "Kick player with specified id for any reason");
            Console.RegisterCommand("status", "", ConsoleStatus, ConfigFlags.SERVER, "List players");
            Console.RegisterCommand("shutdown", "", ConsoleShutdown, ConfigFlags.SERVER, "Shut down");
            Console.RegisterCommand("logout", "", ConsoleLogout, ConfigFlags.SERVER, "Logout of rcon");
            Console.RegisterCommand("reload", "", ConsoleReload, ConfigFlags.SERVER, "Reload the map");

            /*
                Console()->Register("kick", "i?r", CFGFLAG_SERVER, ConKick, this, "Kick player with specified id for any reason");
	            Console()->Register("status", "", CFGFLAG_SERVER, ConStatus, this, "List players");
	            Console()->Register("shutdown", "", CFGFLAG_SERVER, ConShutdown, this, "Shut down");
	            Console()->Register("logout", "", CFGFLAG_SERVER, ConLogout, this, "Logout of rcon");

	            Console()->Register("record", "?s", CFGFLAG_SERVER|CFGFLAG_STORE, ConRecord, this, "Record to a file");
	            Console()->Register("stoprecord", "", CFGFLAG_SERVER, ConStopRecord, this, "Stop recording");

	            Console()->Register("reload", "", CFGFLAG_SERVER, ConMapReload, this, "Reload the map");

	            Console()->Chain("sv_name", ConchainSpecialInfoupdate, this);
	            Console()->Chain("password", ConchainSpecialInfoupdate, this);

	            Console()->Chain("sv_max_clients_per_ip", ConchainMaxclientsperipUpdate, this);
	            Console()->Chain("mod_command", ConchainModCommandUpdate, this);
	            Console()->Chain("console_output_level", ConchainConsoleOutputLevelUpdate, this);
            */
        }

        protected override void ConsoleReload(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        protected override void ConsoleLogout(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        protected override void ConsoleShutdown(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        protected override void ConsoleStatus(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }

        protected override void ConsoleKick(ConsoleResult result, object data)
        {
            throw new NotImplementedException();
        }
    }
}

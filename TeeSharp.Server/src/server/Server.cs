using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
using SnapshotItem = TeeSharp.Common.Snapshots.SnapshotItem;

namespace TeeSharp.Server
{
    public class ServerKernelConfig : DefaultKernelConfig
    {
        public override void Load(IKernel kernel)
        {
            base.Load(kernel);

            // singletons
            kernel.Bind<BaseVotes>().To<Votes>().AsSingleton();
            kernel.Bind<BaseServer>().To<Server>().AsSingleton();
            kernel.Bind<BaseConfig>().To<ServerConfig>().AsSingleton();
            kernel.Bind<BaseNetworkBan>().To<NetworkBan>().AsSingleton();
            kernel.Bind<BaseGameContext>().To<GameContext>().AsSingleton();
            kernel.Bind<BaseEvents>().To<Events>().AsSingleton();
            kernel.Bind<BaseStorage>().To<Storage>().AsSingleton();
            kernel.Bind<BaseNetworkServer>().To<NetworkServer>().AsSingleton();
            kernel.Bind<BaseGameConsole>().To<GameConsole>().AsSingleton();
            kernel.Bind<BaseRegister>().To<Register>().AsSingleton();
            kernel.Bind<BaseGameWorld>().To<GameWorld>().AsSingleton();
            kernel.Bind<BaseTuningParams>().To<TuningParams>().AsSingleton();
            kernel.Bind<BaseGameMsgUnpacker>().To<GameMsgUnpacker>().AsSingleton();
            kernel.Bind<BaseMapCollision>().To<MapCollision>().AsSingleton();
            kernel.Bind<BaseMapLayers>().To<MapLayers>().AsSingleton();

            kernel.Bind<BaseServerClient>().To<ServerClient>();
            kernel.Bind<BaseNetworkConnection>().To<NetworkConnection>();
            kernel.Bind<BaseChunkReceiver>().To<ChunkReceiver>();
            kernel.Bind<BasePlayer>().To<Player>();
        }
    }

    public class Server : BaseServer
    {
        public override int MaxClients => Clients.Length;
        public override int TickSpeed { get; } = SERVER_TICK_SPEED;
        public override int[] IdMap { get; protected set; }
        protected override SnapshotIdPool SnapshotIdPool { get; set; }

        private int[] _lastSent;
        private int[] _lastAsk;
        private int[] _lastAskTick;

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

            Storage.Init("TeeSharp", StorageType.SERVER);

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

            Clients = new BaseServerClient[NetworkServer.Config.MaxClients];
            IdMap = new int[Clients.Length * VANILLA_MAX_CLIENTS];

            for (var i = 0; i < Clients.Length; i++)
                Clients[i] = Kernel.Get<BaseServerClient>();

            NetworkServer.SetCallbacks(NewClientCallback, DelClientCallback);
            Console.Print(OutputLevel.STANDARD, "server", $"server name is '{Config["SvName"]}'");
            GameContext.OnInit();

            StartTime = Time.Get();
            IsRunning = true;

            _lastSent = new int[NetworkServer.Config.MaxClients];
            _lastAsk = new int[NetworkServer.Config.MaxClients];
            _lastAskTick = new int[NetworkServer.Config.MaxClients];

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
                        if (Clients[clientId].State != ServerClientState.IN_GAME)
                            continue;

                        for (var inputIndex = 0; inputIndex < Clients[clientId].Inputs.Length; inputIndex++)
                        {
                            if (Clients[clientId].Inputs[inputIndex].Tick == Tick)
                            {
                                GameContext.OnClientPredictedInput(clientId,
                                    Clients[clientId].Inputs[inputIndex].PlayerInput);
                                break;
                            }
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

                Register.RegisterUpdate(NetworkServer.NetType());
                PumpNetwork();

                Thread.Sleep(5);
            }

            for (var i = 0; i < Clients.Length; i++)
            {
                if (Clients[i].State != ServerClientState.EMPTY)
                    NetworkServer.Drop(i, "Server shutdown");
            }

            GameContext.OnShutdown();
        }
        
        public override void SetClientName(int clientId, string name)
        {
            if (Clients[clientId].State < ServerClientState.READY || string.IsNullOrEmpty(name))
            {
                return;
            }

            name = name.Limit(16).Trim();
            if (string.IsNullOrWhiteSpace(name))
                return;

            {
                var cleanName = new StringBuilder(name);
                for (var i = 0; i < cleanName.Length; i++)
                {
                    if (cleanName[i] < 32)
                        cleanName[i] = ' ';
                }
                name = cleanName.ToString();
            }

            if (GetClientName(clientId) == name)
                return;

            bool AlreadyUsed(string newName)
            {
                for (var i = 0; i < Clients.Length; i++)
                {
                    if (i != clientId && 
                        Clients[i].State >= ServerClientState.READY &&
                        GetClientName(i) == newName)
                    {
                        return true;
                    }
                }

                Clients[clientId].PlayerName = newName;
                return false;
            }

            if (AlreadyUsed(name))
            {
                for (var i = 0;; i++)
                {
                    var nameTry = $"({i}) {name}";
                    if (!AlreadyUsed(nameTry))
                        break;
                }
            }
        }

        public override void SetClientClan(int clientId, string clan)
        {
            if (Clients[clientId].State < ServerClientState.READY ||
                string.IsNullOrEmpty(clan))
            {
                return;
            }

            Clients[clientId].PlayerClan = clan;
        }

        public override void SetClientCountry(int clientId, int country)
        {
            if (Clients[clientId].State < ServerClientState.READY)
                return;
            Clients[clientId].PlayerCountry = country;
        }

        public override bool GetClientInfo(int clientId, out ClientInfo info)
        {
            if (Clients[clientId].State != ServerClientState.IN_GAME)
            {
                info = null;
                return false;
            }

            info = new ClientInfo
            {
                Name = GetClientName(clientId),
                Latency = Clients[clientId].Latency,
                ClientVersion = GameContext.Players[clientId].ClientVersion,
            };

            return true;
        }

        public override string GetClientName(int clientId)
        {
            if (Clients[clientId].State == ServerClientState.EMPTY)
                return "(invalid)";
            if (Clients[clientId].State == ServerClientState.IN_GAME)
                return Clients[clientId].PlayerName;
            return "(connecting)";
        }

        public override string GetClientClan(int clientId)
        {
            if (Clients[clientId].State == ServerClientState.EMPTY)
                return "";
            if (Clients[clientId].State == ServerClientState.IN_GAME)
                return Clients[clientId].PlayerClan;
            return "";
        }

        public override int GetClientCountry(int clientId)
        {
            if (Clients[clientId].State == ServerClientState.EMPTY)
                return -1;
            if (Clients[clientId].State == ServerClientState.IN_GAME)
                return Clients[clientId].PlayerCountry;
            return -1;
        }

        public override int GetClientScore(int clientId)
        {
            return GameContext.GameController.GetPlayerScore(clientId);
        }

        public override bool ClientInGame(int clientId)
        {
            return Clients[clientId].State == ServerClientState.IN_GAME;
        }

        public override bool SendMsg(MsgPacker msg, MsgFlags flags, int clientId)
        {
            return SendMsgEx(msg, flags, clientId, false);
        }

        public override bool SendMsgEx(MsgPacker msg, MsgFlags flags, int clientId, bool system)
        {
            if (msg == null)
                return false;

            var packet = new Chunk()
            {
                ClientId = clientId,
                DataSize = msg.Size(),
                Data = msg.Data(),
            };

            packet.Data[0] <<= 1;
            if (system)
                packet.Data[0] |= 1;

            if (flags.HasFlag(MsgFlags.Vital))
                packet.Flags |= SendFlags.VITAL;
            if (flags.HasFlag(MsgFlags.Flush))
                packet.Flags |= SendFlags.FLUSH;

            if (!flags.HasFlag(MsgFlags.NoSend))
            {
                if (clientId == -1)
                {
                    for (var i = 0; i < Clients.Length; i++)
                    {
                        if (Clients[i].State != ServerClientState.IN_GAME)
                            continue;

                        packet.ClientId = i;
                        NetworkServer.Send(packet);
                    }
                }
                else
                {
                    NetworkServer.Send(packet);
                }
            }

            return true;
        }

        public override bool SendPackMsg<T>(T msg, MsgFlags flags, int clientId)
        {
            var result = true;

            if (clientId == -1)
            {
                for (var i = 0; i < Clients.Length; i++)
                {
                    if (ClientInGame(i))
                    {
                        result &= SendPackMsgBody<T>(msg, flags, i);
                    }
                }
            }
            else
            {
                return SendPackMsgBody<T>(msg, flags, clientId);
            }

            return result;
        }

        public override bool AddSnapItem<T>(T item, int id)
        {
            Debug.Assert(id >= 0 && id <= 65535, "incorrect id");
            return id >= 0 && SnapshotBuilder.AddItem(item, id);
        }

        public override T SnapObject<T>(int id)
        {
            Debug.Assert(id >= 0 && id <= 65535, "incorrect id");

            return id < 0
                ? null
                : SnapshotBuilder.NewObject<T>(id);
        }

        protected override bool SendPackMsgBody<T>(T msg, MsgFlags flags, int clientId)
        {
            if (msg is GameMsg_SvEmoticon)
                return SendPackMsgTranslate(msg as GameMsg_SvEmoticon, flags, clientId);
            if (msg is GameMsg_SvChat)
                return SendPackMsgTranslate(msg as GameMsg_SvChat, flags, clientId);
            if (msg is GameMsg_SvKillMsg)
                return SendPackMsgTranslate(msg as GameMsg_SvKillMsg, flags, clientId);
            return SendPackMsgTranslate(msg, flags, clientId);
        }

        protected override bool SendPackMsgTranslate(GameMsg_SvEmoticon msg, MsgFlags flags, int clientId)
        {
            var copy = new GameMsg_SvEmoticon
            {
                ClientId = msg.ClientId,
                Emoticon = msg.Emoticon
            };

            return Translate(ref copy.ClientId, clientId) && 
                   SendPackMsgOne(copy, flags, clientId);
        }

        protected override bool SendPackMsgTranslate(GameMsg_SvChat msg, MsgFlags flags, int clientId)
        {
            var copy = new GameMsg_SvChat
            {
                ClientId = msg.ClientId,
                Message = msg.Message,
                IsTeam = msg.IsTeam
            };

            if (copy.ClientId >= 0 && !Translate(ref copy.ClientId, clientId))
            {
                copy.Message = $"{GetClientName(copy.ClientId)}: {copy.Message}";
                copy.ClientId = VANILLA_MAX_CLIENTS - 1;
            }

            return SendPackMsgOne(copy, flags, clientId);
        }

        protected override bool SendPackMsgTranslate(GameMsg_SvKillMsg msg, 
            MsgFlags flags, int clientId)
        {
            var copy = new GameMsg_SvKillMsg
            {
                Weapon = msg.Weapon,
                Killer = msg.Killer,
                ModeSpecial = msg.ModeSpecial,
                Victim = msg.Victim
            };

            if (!Translate(ref copy.Victim, clientId)) return false;
            if (!Translate(ref copy.Killer, clientId)) copy.Killer = copy.Victim;

            return SendPackMsgOne(copy, flags, clientId);
        }

        protected override bool SendPackMsgTranslate(BaseGameMessage msg, 
            MsgFlags flags, int clientId)
        {
            return SendPackMsgOne(msg, flags, clientId);
        }

        protected override bool SendPackMsgOne(BaseGameMessage msg, 
            MsgFlags flags, int clientId)
        {
            var packer = new MsgPacker((int) msg.Type);
            if (msg.PackError(packer))
                return false;
            return SendMsg(packer, flags, clientId);
        }

        public override bool Translate(ref int targetId, int clientId)
        {
            if (!GetClientInfo(clientId, out var info))
                return false;

            if (info.ClientVersion >= ClientVersion.DDNET_OLD)
                return true;

            var map = GetIdMap(clientId);

            for (var i = 0; i < VANILLA_MAX_CLIENTS; i++)
            {
                if (targetId == IdMap[map + i])
                {
                    targetId = i;
                    return true;
                }
            }

            return false;
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
            var bindAddr = NetworkCore.GetLocalIP(AddressFamily.InterNetwork);
            if (!string.IsNullOrWhiteSpace(Config["Bindaddr"]))
                bindAddr = IPAddress.Parse(Config["Bindaddr"]);

            var networkConfig = new NetworkServerConfig
            {
                BindEndPoint = new IPEndPoint(bindAddr, Config["SvPort"]),
                MaxClientsPerIp = Config["SvMaxClientsPerIP"],
                MaxClients = Config["SvMaxClients"]
            };

            if (!NetworkServer.Open(networkConfig))
            {
                Debug.Error("server", $"couldn't open socket. port {networkConfig.BindEndPoint.Port} might already be in use");
                return false;
            }

            Debug.Log("server", $"network server running at: {networkConfig.BindEndPoint}");
            return true;
        }

        protected override void ProcessClientPacket(Chunk packet)
        {
            var clientId = packet.ClientId;
            var unpacker = new UnPacker();
            unpacker.Reset(packet.Data, packet.DataSize);

            var msg = unpacker.GetInt();
            var isSystemMsg = (msg & 1) != 0;
            msg >>= 1;

            if (unpacker.Error)
                return;

            if (Config["SvNetlimit"] && 
                !(isSystemMsg && msg == (int)NetworkMessages.CL_REQUEST_MAP_DATA))
            {
                var now = Time.Get();
                var diff = now - Clients[clientId].TrafficSince;
                var alpha = Config["SvNetlimitAlpha"] / 100f;
                var limit = (float) Config["SvNetlimit"] * 1024 / Time.Freq();

                if (Clients[clientId].Traffic > limit)
                {
                    NetworkBan.BanAddr(packet.EndPoint, 600, "Stressing network");
                    return;
                }

                if (diff > 100)
                {
                    Clients[clientId].Traffic = (int) (alpha * ((float) packet.DataSize / diff) +
                                                      (1.0f - alpha) * Clients[clientId].Traffic);
                    Clients[clientId].TrafficSince = now;
                }
            }

            if (isSystemMsg)
            {
                var networkMsg = (NetworkMessages) msg;
                switch (networkMsg)
                {
                    case NetworkMessages.CL_INFO:
                        NetMsgInfo(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.CL_REQUEST_MAP_DATA:
                        NetMsgRequestMapData(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.CL_READY:
                        NetMsgReady(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.CL_ENTERGAME:
                        NetMsgEnterGame(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.CL_INPUT:
                        NetMsgInput(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.CL_RCON_CMD:
                        NetMsgRconCmd(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.CL_RCON_AUTH:
                        NetMsgRconAuth(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.PING:
                        NetMsgPing(packet, unpacker, clientId);
                        break;
                    default:
                        Console.Print(OutputLevel.DEBUG, "server", $"strange message clientId={clientId} msg={msg} data_size={packet.DataSize}");
                        break;
                }
            }
            else if (Clients[clientId].State >= ServerClientState.READY)
            {
                GameContext.OnMessage(msg, unpacker, clientId);
            }
        }

        protected override void NetMsgPing(Chunk packet, UnPacker unPacker, int clientId)
        {
            var msg = new MsgPacker((int) NetworkMessages.PING_REPLY);
            SendMsgEx(msg, 0, clientId, true);
        }

        protected override void NetMsgRconAuth(Chunk packet, UnPacker unPacker, int clientId)
        {
            var login = unPacker.GetString();
            var password = unPacker.GetString();

            SendRconLine(clientId, password);
            if (!packet.Flags.HasFlag(SendFlags.VITAL) || unPacker.Error)
                return;

            if (string.IsNullOrEmpty(Config["SvRconPassword"]) &&
                string.IsNullOrEmpty(Config["SvRconModPassword"]))
            {
                SendRconLine(clientId, "No rcon password set on server. Set sv_rcon_password and/or sv_rcon_mod_password to enable the remote console.");
            }

            var authed = false;
            if (password == Config["SvRconPassword"])
            {
                authed = true;
            }
            else if (password == Config["SvRconModPassword"])
            {
                authed = true;
            }
            
            if (authed)
            {
                var msg = new MsgPacker((int) NetworkMessages.SV_RCON_AUTH_STATUS);
                msg.AddInt(1);
                msg.AddInt(1);
                SendMsgEx(msg, MsgFlags.Vital, clientId, true);
            }
        }

        protected override void NetMsgRconCmd(Chunk packet, UnPacker unPacker, int clientId)
        {
            
        }

        protected override void NetMsgInput(Chunk packet, UnPacker unPacker, int clientId)
        {
            Clients[clientId].LastAckedSnapshot = unPacker.GetInt();
            var intendedTick = unPacker.GetInt();
            var size = unPacker.GetInt();

            if (unPacker.Error || size / sizeof(int) > BaseServerClient.MAX_INPUT_SIZE)
                return;

            if (Clients[clientId].LastAckedSnapshot > 0)
                Clients[clientId].SnapRate = SnapRate.FULL;

            if (Clients[clientId].SnapshotStorage.Get(
                Clients[clientId].LastAckedSnapshot,
                out var tagTime,
                out var _))
            {
                Clients[clientId].Latency =
                    (int) (((Time.Get() - tagTime) * 1000) / Time.Freq());
            }

            if (intendedTick > Clients[clientId].LastInputTick)
            {
                var timeLeft = (TickStartTime(intendedTick) - Time.Get()) * 1000 / Time.Freq();
                var msg = new MsgPacker((int)NetworkMessages.SV_INPUT_TIMING);
                msg.AddInt(intendedTick);
                msg.AddInt((int) timeLeft);
                SendMsgEx(msg, MsgFlags.None, clientId, true);
            }

            Clients[clientId].LastInputTick = intendedTick;
            var input = Clients[clientId].Inputs[Clients[clientId].CurrentInput];

            if (intendedTick <= Tick)
                intendedTick = Tick + 1;

            var data = new int[size / sizeof(int)];
            for (var i = 0; i < data.Length; i++)
                data[i] = unPacker.GetInt();

            input.Tick = intendedTick;
            input.PlayerInput.Deserialize(data, 0);

            Clients[clientId].CurrentInput++;
            Clients[clientId].CurrentInput %= Clients[clientId].Inputs.Length;

            if (Clients[clientId].State == ServerClientState.IN_GAME)
                GameContext.OnClientDirectInput(clientId, input.PlayerInput);
        }

        protected override void NetMsgEnterGame(Chunk packet, UnPacker unPacker, int clientId)
        {
            if (Clients[clientId].State != ServerClientState.READY || 
                !GameContext.IsClientReady(clientId))
            {
                return;
            }

            Console.Print(OutputLevel.STANDARD, "server", $"player has entered the game. ClientID={clientId} addr={NetworkServer.ClientEndPoint(clientId)}");
            Clients[clientId].State = ServerClientState.IN_GAME;
            GameContext.OnClientEnter(clientId);
        }

        protected override void NetMsgReady(Chunk packet, UnPacker unPacker, int clientId)
        {
            if (Clients[clientId].State != ServerClientState.CONNECTING)
                return;

            Console.Print(OutputLevel.ADDINFO, "server", $"player is ready. ClientID={clientId} addr={NetworkServer.ClientEndPoint(clientId)}");
            Clients[clientId].State = ServerClientState.READY;
            GameContext.OnClientConnected(clientId);

            var msg = new MsgPacker((int) NetworkMessages.SV_CON_READY);
            SendMsgEx(msg, MsgFlags.Vital | MsgFlags.Flush, clientId, true);
        }

        protected override void NetMsgRequestMapData(Chunk packet, UnPacker unPacker, int clientId)
        {
            if (Clients[clientId].State < ServerClientState.CONNECTING)
                return;

            var chunk = unPacker.GetInt();
            var chunkSize = 1024 - 128;
            var offset = chunk * chunkSize;
            var last = 0;

            _lastAsk[clientId] = chunk;
            _lastAskTick[clientId] = Tick;

            if (chunk == 0)
                _lastSent[clientId] = 0;

            if (chunk < 0 || offset > CurrentMap.Size)
                return;

            if (offset + chunkSize >= CurrentMap.Size)
            {
                chunkSize = CurrentMap.Size - offset;
                last = 1;

                if (chunkSize < 0)
                    chunkSize = 0;
            }

            if (_lastSent[clientId] < chunk + Config["SvMapWindow"] &&
                Config["SvFastDownload"])
            {
                return;
            }

            SendMapData(last, chunk, chunkSize, offset, clientId);
        }

        protected override void NetMsgInfo(Chunk packet, UnPacker unPacker, int clientId)
        {
            if (Clients[clientId].State != ServerClientState.AUTH)
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

            if (clientId >= NetworkServer.Config.MaxClients - Config["SvReservedSlots"] &&
                !string.IsNullOrEmpty(Config["SvReservedSlotsPass"]) &&
                password != Config["SvReservedSlotsPass"])
            {
                NetworkServer.Drop(clientId, "This server is full");
                return;
            }

            Clients[clientId].State = ServerClientState.CONNECTING;
            SendMap(clientId);
        }

        protected override void PumpNetwork()
        {
            NetworkServer.Update();

            Chunk packet = null;
            uint token = 0;

            while (NetworkServer.Receive(ref packet, ref token))
            {
                if (packet.ClientId == -1)
                {
                    if (packet.DataSize == MasterServerPackets.GetInfo.Length + 1 &&
                        packet.Data.ArrayCompare(
                            MasterServerPackets.GetInfo,
                            MasterServerPackets.GetInfo.Length))
                    {
                        SendServerInfo(
                            packet.EndPoint,
                            packet.Data[MasterServerPackets.GetInfo.Length],
                            false
                        );
                    }
                    else if (packet.DataSize == MasterServerPackets.SERVERBROWSE_GETINFO_64_LEGACY.Length + 1 &&
                             packet.Data.ArrayCompare(
                                 MasterServerPackets.SERVERBROWSE_GETINFO_64_LEGACY,
                                 MasterServerPackets.SERVERBROWSE_GETINFO_64_LEGACY.Length))
                    {
                        SendServerInfo(
                            packet.EndPoint,
                            packet.Data[MasterServerPackets.GetInfo.Length],
                            true
                        );
                    }

                    continue;
                }

                ProcessClientPacket(packet);
            }

            if (Config["SvFastDownload"])
            {
                for (var i = 0; i < Clients.Length; i++)
                {
                    if (Clients[i].State != ServerClientState.CONNECTING)
                        continue;

                    if (_lastAskTick[i] < Tick - TickSpeed)
                    {
                        _lastSent[i] = _lastAsk[i];
                        _lastAskTick[i] = Tick;
                    }

                    if (_lastAsk[i] < _lastSent[i] - Config["SvMapWindow"])
                        continue;

                    var chunk = _lastSent[i]++;
                    var chunkSize = 1024 - 128;
                    var offset = chunk * chunkSize;
                    var last = 0;

                    // drop faulty map data requests
                    if (chunk < 0 || offset > CurrentMap.Size)
                        continue;

                    if (offset + chunkSize >= CurrentMap.Size)
                    {
                        chunkSize = CurrentMap.Size - offset;
                        if (chunkSize < 0)
                            chunkSize = 0;
                        last = 1;
                    }

                    SendMapData(last, chunk, chunkSize, offset, i);
                }
            }

            NetworkBan.Update();
        }

        private void SendMapData(int last, int chunk, int chunkSize, 
            int offset, int clientId)
        {
            var msg = new MsgPacker((int) NetworkMessages.SV_MAP_DATA);
            msg.AddInt(last);
            msg.AddInt((int)CurrentMap.CRC);
            msg.AddInt(chunk);
            msg.AddInt(chunkSize);
            msg.AddRaw(CurrentMap.RawData, offset, chunkSize);
            SendMsgEx(msg, MsgFlags.Flush, clientId, true);

            Debug.Log("server", $"sending chunk {chunk} with size {chunkSize}");
        }

        protected override void DoSnapshot()
        {
            GameContext.OnBeforeSnapshots();

            for (var i = 0; i < Clients.Length; i++)
            {
                // client must be ingame to recive snapshots
                if (Clients[i].State != ServerClientState.IN_GAME)
                    continue;

                // this client is trying to recover, don't spam snapshots
                if (Clients[i].SnapRate == SnapRate.RECOVER && Tick % TickSpeed != 0)
                    continue;
                
                // this client is trying to recover, don't spam snapshots
                if (Clients[i].SnapRate == SnapRate.INIT && Tick % 10 != 0)
                    continue;
                
                SnapshotBuilder.StartBuild();
                GameContext.OnSnapshot(i);
                var now = Time.Get();
                var snapshot =  SnapshotBuilder.EndBuild();
                var crc = snapshot.Crc();

                Clients[i].SnapshotStorage.PurgeUntil(Tick - TickSpeed * 3);
                Clients[i].SnapshotStorage.Add(Tick, now, snapshot);

                var deltaTick = -1;

                if (Clients[i].SnapshotStorage.Get(Clients[i].LastAckedSnapshot,
                    out var _, out var deltaSnapshot))
                {
                    deltaTick = Clients[i].LastAckedSnapshot;
                }
                else
                {
                    deltaSnapshot = new Snapshot(new SnapshotItem[0], 0);
                    if (Clients[i].SnapRate == SnapRate.FULL)
                        Clients[i].SnapRate = SnapRate.RECOVER;
                }

                var deltaData = new int[SnapshotBuilder.MAX_SNAPSHOT_SIZE / sizeof(int)];
                var deltaSize = SnapshotDelta.CreateDelta(deltaSnapshot, snapshot, deltaData);

                if (deltaSize == 0)
                {
                    var msg = new MsgPacker((int) NetworkMessages.SV_SNAPEMPTY);
                    msg.AddInt(Tick);
                    msg.AddInt(Tick - deltaTick);
                    SendMsgEx(msg, MsgFlags.Flush, i, true);
                    continue;
                }

                var snapData = new byte[SnapshotBuilder.MAX_SNAPSHOT_SIZE];
                var snapshotSize = IntCompression.Compress(deltaData, 0, 
                    deltaSize, snapData, 0); // Compress size in bytes
                var numPackets = (snapshotSize + Snapshot.MAX_SNAPSHOT_PACKSIZE - 1) / Snapshot.MAX_SNAPSHOT_PACKSIZE;

                for (int n = 0, left = snapshotSize; left != 0; n++)
                {
                    var chunk = left < Snapshot.MAX_SNAPSHOT_PACKSIZE ? left : Snapshot.MAX_SNAPSHOT_PACKSIZE;
                    left -= chunk;

                    if (numPackets == 1)
                    {
                        var msg = new MsgPacker((int) NetworkMessages.SV_SNAPSINGLE);
                        msg.AddInt(Tick);
                        msg.AddInt(Tick - deltaTick);
                        msg.AddInt(crc);
                        msg.AddInt(chunk);
                        msg.AddRaw(snapData, n * Snapshot.MAX_SNAPSHOT_PACKSIZE, chunk);
                        SendMsgEx(msg, MsgFlags.Flush, i, true);
                    }
                    else
                    {
                        var msg = new MsgPacker((int) NetworkMessages.SV_SNAP);
                        msg.AddInt(Tick);
                        msg.AddInt(Tick - deltaTick);
                        msg.AddInt(numPackets);
                        msg.AddInt(n);
                        msg.AddInt(crc);
                        msg.AddInt(chunk);
                        msg.AddRaw(snapData, n * Snapshot.MAX_SNAPSHOT_PACKSIZE, chunk);
                        SendMsgEx(msg, MsgFlags.Flush, i, true);
                    }
                }
            }

            GameContext.OnAfterSnapshots();
        }

        protected override long TickStartTime(int tick)
        {
            return StartTime + (Time.Freq() * tick) / TickSpeed;

        }

        protected override void DelClientCallback(int clientId, string reason)
        {
            if (!string.IsNullOrWhiteSpace(reason))
            {
                
            }

            Debug.Log("clients", $"client dropped. cid={clientId} addr={NetworkServer.ClientEndPoint(clientId)} reason='{reason}'");
            if (Clients[clientId].State >= ServerClientState.READY)
                GameContext.OnClientDrop(clientId, reason);

            Clients[clientId].State = ServerClientState.EMPTY;
        }

        protected override void NewClientCallback(int clientid, bool legacy)
        {
            Clients[clientid].State = legacy == false ?
                ServerClientState.AUTH :
                ServerClientState.CONNECTING;
            Clients[clientid].Reset();

            if (legacy)
                SendMap(clientid);
        }

        protected override bool LoadMap(string mapName)
        {
            mapName = Path.GetFileNameWithoutExtension(mapName);
            var path = $"maps/{mapName}.map";

            Console.Print(OutputLevel.DEBUG, "map", $"loading map='{path}'");

            using (var stream = Storage.OpenFile(path, FileAccess.Read))
            {
                if (stream == null)
                {
                    Console.Print(OutputLevel.DEBUG, "map", $"could not open map='{path}'");
                    return false;
                }

                CurrentMap = MapContainer.Load(stream, out var error);
                if (CurrentMap == null)
                {
                    Console.Print(OutputLevel.DEBUG, "map", $"error with load map='{path}' ({error})");
                    return false;
                }
                CurrentMap.MapName = mapName;
                Console.Print(OutputLevel.DEBUG, "map", $"successful load map='{path}' ({error})");

                return true;
            }
        }

        protected override void SendMap(int clientId)
        {
            _lastSent[clientId] = 0;
            _lastAsk[clientId] = 0;
            _lastAskTick[clientId] = Tick;

            var msg = new MsgPacker((int) NetworkMessages.SV_MAP_CHANGE);
            msg.AddString(CurrentMap.MapName);
            msg.AddInt((int) CurrentMap.CRC);
            msg.AddInt(CurrentMap.Size);
            SendMsgEx(msg, MsgFlags.Vital | MsgFlags.Flush, clientId, true);
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

        protected override void SendServerInfo(IPEndPoint endPoint, int token, bool showMore, int offset = 0)
        {
            while (true)
            {
                var packer = new Packer();
                var playersCount = 0;
                var clientsCount = 0;
                var maxClients = NetworkServer.Config.MaxClients;

                for (var i = 0; i < Clients.Length; i++)
                {
                    if (Clients[i].State == ServerClientState.EMPTY)
                        continue;

                    if (!GameContext.IsClientSpectator(i))
                        playersCount++;
                    clientsCount++;
                }

                packer.AddRaw(showMore
                    ? MasterServerPackets.SERVERBROWSE_INFO_64_LEGACY
                    : MasterServerPackets.Info);

                packer.AddString(token.ToString(), 6);
                packer.AddString(GameContext.GameVersion, 32);

                if (showMore)
                {
                    packer.AddString(Config["SvName"], 256);
                }
                else
                {
                    packer.AddString(maxClients <= VANILLA_MAX_CLIENTS
                        ? Config["SvName"]
                        : $"{Config["SvName"]} [{clientsCount}/{maxClients}]", 64);
                }

                packer.AddString(CurrentMap.MapName, 32);
                packer.AddString(GameContext.GameController.GameType, 16);
                packer.AddInt(string.IsNullOrWhiteSpace(Config["Password"])
                    ? 0
                    : 1);

                if (!showMore)
                {
                    if (clientsCount >= VANILLA_MAX_CLIENTS)
                    {
                        if (clientsCount < maxClients)
                            clientsCount = VANILLA_MAX_CLIENTS - 1;
                        else
                            clientsCount = VANILLA_MAX_CLIENTS;
                    }

                    if (maxClients > VANILLA_MAX_CLIENTS)
                        maxClients = VANILLA_MAX_CLIENTS;
                }

                if (playersCount > clientsCount)
                    playersCount = clientsCount;

                // num players
                packer.AddString(playersCount.ToString(), 3);
                // max players
                packer.AddString((maxClients - Config["SvSpectatorSlots"]).ToString(), 3);
                // num clients
                packer.AddString(clientsCount.ToString(), 3);
                // max clients
                packer.AddString(maxClients.ToString(), 3);

                if (showMore)
                    packer.AddInt(offset);

                var clientsPerPacket = showMore ? 24 : VANILLA_MAX_CLIENTS;
                var skip = offset;
                var take = clientsPerPacket;

                for (var i = 0; i < Clients.Length; i++)
                {
                    if (Clients[i].State == ServerClientState.EMPTY)
                        continue;

                    if (skip-- > 0)
                        continue;
                    if (--take < 0)
                        break;

                    // client name
                    packer.AddString(GetClientName(i), 16);
                    // client clan
                    packer.AddString(GetClientClan(i), 12);
                    // client country
                    packer.AddString(GetClientCountry(i).ToString(), 6);
                    // client score
                    packer.AddString(GetClientScore(i).ToString(), 6);
                    // client state
                    packer.AddString(GameContext.IsClientSpectator(i)
                        ? "0"
                        : "1", 2);
                }

                NetworkServer.Send(new Chunk
                {
                    ClientId = -1,
                    EndPoint = endPoint,
                    DataSize = packer.Size(),
                    Data = packer.Data(),
                    Flags = SendFlags.CONNLESS
                });

                if (showMore && take < 0)
                {
                    offset = offset + clientsPerPacket;
                    continue;
                }
                break;
            }
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

        protected override void SendRconLine(int clientId, string line)
        {
            var packer = new MsgPacker((int) NetworkMessages.SV_RCON_LINE);
            packer.AddString(line, 512);
            SendMsgEx(packer, MsgFlags.Vital, clientId, true);
        }

        protected override void SendRconLineAuthed(string message, object data)
        {
            
        }
    }
}

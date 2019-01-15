using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Game;
using TeeSharp.Common.Snapshots;
using TeeSharp.Common.Storage;
using TeeSharp.Core;
using TeeSharp.Core.Extensions;
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
        public override event ClientEvent PlayerReady;
        public override event ClientEvent PlayerEnter;
        public override event ClientDisconnectEvent PlayerDisconnected;

        public override int MaxClients => 64;
        public override int MaxPlayers => 16;
        public override int TickSpeed => 50;

        public override void Init(string[] args)
        {
            Tick = 0;
            StartTime = 0;
            SendRconCommandsClients = new Queue<int>();
            GameTypes = new Dictionary<string, Type>();
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
            Config.Init(ConfigFlags.Server | ConfigFlags.Econ);
            Console.Init();
            Console.CommandAdded += ConsoleOnCommandAdded;
            PrintCallbackInfo = Console.RegisterPrintCallback(
                (OutputLevel) Config["ConsoleOutputLevel"].AsInt(), OnConsolePrint);
            NetworkServer.Init();

            GameContext.BeforeInit();

            var useDefaultConfig = args.Any(a => a == "--default" || a == "-d");
            if (useDefaultConfig)
            {
            }
            else
            {
                RegisterConsoleCommands();
                Console.ExecuteFile("autoexec.cfg");
                Console.ParseArguments(args);
            }

            Config.RestoreString();
        }

        protected override void ConsoleOnCommandAdded(ConsoleCommand command)
        {
            command.AccessLevel = BaseServerClient.AuthedAdmin;
        }

        protected override void RandomRconPassword()
        {
            const int PasswordLength = 6;
            const string PasswordChars = "ABCDEFGHKLMNPRSTUVWXYZabcdefghjkmnopqt23456789";

            Debug.Assert(PasswordLength % 2 == 0, "Need an even password length");

            var password = new StringBuilder(PasswordLength);
            var random = new ushort[PasswordLength / 2];

            Secure.RandomFill(random);

            for (var i = 0; i < PasswordLength / 2; i++)
            {
                var randomNumber = random[i] % 2048;
                password.Append(PasswordChars[randomNumber / PasswordChars.Length]);
                password.Append(PasswordChars[randomNumber % PasswordChars.Length]);
            }

            ((ConfigString) Config["SvRconPassword"]).Value = password.ToString();
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
            GameContext.Init();
            GameContext.RegisterCommandsUpdates();
            RegisterConsoleUpdates();

            StartTime = Time.Get();
            IsRunning = true;

            if (string.IsNullOrEmpty(Config["SvRconPassword"]))
            {
                RandomRconPassword();
                Debug.Assert(false, "+-------------------------+");
                Debug.Assert(false, $"| rcon password: '{Config["SvRconPassword"]}' |");
                Debug.Assert(false, "+-------------------------+");
            }

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
                    SendClientRconCommands();
                }

                //Register.RegisterUpdate(NetworkServer.NetType());
                PumpNetwork();

                Thread.Sleep(5);
            }

            for (var i = 0; i < Clients.Length; i++)
            {
                if (Clients[i].State != ServerClientState.Empty)
                    Kick(i, "Server shutdown");
            }

            GameContext.OnShutdown();
        }

        protected override void SendClientRconCommands()
        {
            if (SendRconCommandsClients.Count == 0)
                return;


            var clientId = SendRconCommandsClients.Peek();
            void Reset()
            {
                if (Clients[clientId].SendCommandsEnumerator != null)
                {
                    Clients[clientId].SendCommandsEnumerator.Dispose();
                    Clients[clientId].SendCommandsEnumerator = null;
                }

                SendRconCommandsClients.Dequeue();
            }

            if (Clients[clientId].State == ServerClientState.Empty ||
                Clients[clientId].SendCommandsEnumerator == null)
            {
                Reset();
                return;
            }
            
            var sended = 0;
            while (sended < 16 && Clients[clientId].SendCommandsEnumerator.MoveNext())
            {
                var command = Clients[clientId].SendCommandsEnumerator.Current.Value;
                SendRconCommandAdd(command, clientId);
                sended++;
            }

            if (sended == 0)
                Reset();
        }

        public override GameController GameController(string gameType)
        {
            Type type;

            try
            {
                type = GameTypes
                    .First(kvp => kvp.Key.Equals(gameType, StringComparison.InvariantCultureIgnoreCase))
                    .Value;
            }
            catch (Exception)
            {
                Debug.Exception("server", $"Gametype '{gameType}' not found");
                throw;
            }
           
            var gameController = (GameController) Activator.CreateInstance(type);
            Debug.Log("server", $"Create gamecontroller '{gameController.GameType}'");
            return gameController;
        }

        public override void AddGametype<T>(string gameType)
        {
            gameType = gameType.ToLower();
            if (GameTypes.ContainsKey(gameType))
                Debug.Warning("server", $"Gametype '{gameType}' already exist");
            GameTypes.Add(gameType, typeof(T));
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
            return Clients[clientId].AccessLevel > 0;
        }

        public override void Kick(int clientId, string reason)
        {
            NetworkServer.Drop(clientId, reason);
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

            NetworkServer.ClientConnected += ClientConnected;
            NetworkServer.ClientDisconnected += ClientDisconnected;
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
                        packer.AddInt(GameContext.GameController.Score(i));
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
            if (IsAuthed(clientId))
                return;

            var password = unPacker.GetString(SanitizeType.SanitizeCC);

            if (!packet.Flags.HasFlag(SendFlags.Vital) || unPacker.Error)
                return;

            // TODO send map list

            if (string.IsNullOrEmpty(Config["SvRconPassword"]) &&
                string.IsNullOrEmpty(Config["SvRconModPassword"]))
            {
                SendRconLine(clientId, "No rcon password set on server. Set sv_rcon_password and/or sv_rcon_mod_password to enable the remote console.");
                return;
            }

            var authed = false;
            var format = string.Empty;
            var authLevel = 0;

            if (!string.IsNullOrEmpty(Config["SvRconPassword"]) && Config["SvRconPassword"] == password)
            {
                authed = true;
                format = $"clientId={clientId} authed 'admin'";
                authLevel = BaseServerClient.AuthedAdmin;
                SendRconLine(clientId, "Admin authentication successful. Full remote console access granted.");
            }
            else if (!string.IsNullOrEmpty(Config["SvRconModPassword"]) && Config["SvRconModPassword"] == password)
            {
                authed = true;
                format = $"clientId={clientId} authed 'moderator'";
                authLevel = BaseServerClient.AuthedModerator;
                SendRconLine(clientId, "Moderator authentication successful. Limited remote console access granted.");
            }
            else if (Config["SvRconMaxTries"])
            {
                Clients[clientId].AuthTries++;
                SendRconLine(clientId,
                    $"Wrong password {Clients[clientId].AuthTries}/{Config["SvRconMaxTries"]}.");

                if (Clients[clientId].AuthTries >= Config["SvRconMaxTries"])
                {
                    if (Config["SvRconBantime"])
                    {
                        NetworkBan.BanAddr(NetworkServer.ClientEndPoint(clientId), Config["SvRconBantime"] * 60,
                            "Too many remote console authentication tries");
                    }
                    else
                    {
                        Kick(clientId, "Too many remote console authentication tries");
                    }
                }
            }
            else
            {
                SendRconLine(clientId, "Wrong password");
            }

            if (authed)
            {
                var msg = new MsgPacker((int) NetworkMessages.ServerRconAuthOn, true);
                SendMsg(msg, MsgFlags.Vital, clientId);
                Console.Print(OutputLevel.Standard, "server", format);

                Clients[clientId].AccessLevel = authLevel;
                Clients[clientId].SendCommandsEnumerator = Console.GetCommands(authLevel);
                SendRconCommandsClients.Enqueue(clientId);
            }
        }

        protected override void NetMsgRconCmd(Chunk packet, UnPacker unPacker, int clientId)
        {
            if (!packet.Flags.HasFlag(SendFlags.Vital) || unPacker.Error)
                return;

            if (!IsAuthed(clientId))
                return;

            var command = unPacker.GetString(SanitizeType.SanitizeCC);
            if (string.IsNullOrEmpty(command))
                return;

            Console.Print(OutputLevel.AddInfo, "server", $"ClientId={clientId} execute rcon command: '{command}'");
            Console.ExecuteLine(command, Clients[clientId].AccessLevel);
        }

        protected override void NetMsgInput(Chunk packet, UnPacker unPacker, int clientId)
        {
            Clients[clientId].LastAckedSnapshot = unPacker.GetInt();

            var intendedTick = unPacker.GetInt();
            var size = unPacker.GetInt();
            var now = Time.Get();

            if (unPacker.Error)
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

            for (var i = 0; i < size / sizeof(int) && i < BaseServerClient.MaxInputSize; i++)
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

            Console.Print(OutputLevel.Standard, "server", $"player has entered the game. ClientId={clientId} addr={NetworkServer.ClientEndPoint(clientId)}");
            Clients[clientId].State = ServerClientState.InGame;
            SendServerInfo(clientId);
            PlayerEnter?.Invoke(clientId);
        }

        protected override void NetMsgReady(Chunk packet, UnPacker unPacker, int clientId)
        {
            if (!packet.Flags.HasFlag(SendFlags.Vital))
                return;

            if (Clients[clientId].State != ServerClientState.Connecting)
                return;

            Console.Print(OutputLevel.AddInfo, "server", $"player is ready. ClientId={clientId} addr={NetworkServer.ClientEndPoint(clientId)}");
            Clients[clientId].State = ServerClientState.Ready;
            PlayerReady?.Invoke(clientId);

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
                Kick(clientId, $"Wrong version. Server is running '{GameContext.NetVersion}' and client '{version}'");
                return;
            }

            var password = unPacker.GetString(SanitizeType.SanitizeCC);
            if (!string.IsNullOrEmpty(Config["Password"]) && password != Config["Password"])
            {
                Kick(clientId, "Wrong password");
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
                PlayerDisconnected?.Invoke(clientId, reason);
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

        protected override void OnConsolePrint(string message, object data)
        {
            for (var i = 0; i < Clients.Length; i++)
            {
                if (!IsAuthed(i))
                    continue;
             
                SendRconLine(i, message);
            }
        }

        protected override void SendRconCommandAdd(ConsoleCommand command, int clientId)
        {
            var msg = new MsgPacker((int) NetworkMessages.ServerRconCommandAdd, true);
            msg.AddString(command.Cmd, ConsoleCommand.MaxCmdLength);
            msg.AddString(command.Description, ConsoleCommand.MaxDescLength);
            msg.AddString(command.Format, ConsoleCommand.MaxParamsLength);
            SendMsg(msg, MsgFlags.Vital, clientId);
        }

        protected override void SendRconCommandRemove(ConsoleCommand command, int clientId)
        {
            var msg = new MsgPacker((int) NetworkMessages.ServerRconCommandRemove, true);
            msg.AddString(command.Cmd, ConsoleCommand.MaxCmdLength);
            SendMsg(msg, MsgFlags.Vital, clientId);
        }

        protected override void RegisterConsoleUpdates()
        {
            Console["sv_name"].Executed += ConsoleSpecialInfoUpdated;
            Console["password"].Executed += ConsoleSpecialInfoUpdated;
            Console["console_output_level"].Executed += ConsoleOutputLevelUpdated;

            Console["sv_max_clients_per_ip"].Executed += ConsoleMaxClientsPerIpUpdated;
            Console["sv_rcon_password"].Executed += ConsoleRconPasswordUpdated;
        }

        protected override void RegisterConsoleCommands()
        {
            Console.AddCommand("mod_command", "s?i", "Specify command accessibility for moderators", ConfigFlags.Server, ConsoleModCommand);
            Console.AddCommand("mod_status", string.Empty, "List all commands which are accessible for moderators", ConfigFlags.Server, ConsoleModStatus);

            Console.AddCommand("status", string.Empty, "List players", ConfigFlags.Server, ConsoleStatus);
            Console.AddCommand("shutdown", string.Empty, "Shut down", ConfigFlags.Server, ConsoleShutdown);
            Console.AddCommand("logout", string.Empty, "Logout of rcon", ConfigFlags.Server, ConsoleLogout);
            Console.AddCommand("reload", string.Empty, "Reload the map", ConfigFlags.Server, ConsoleReload);

            Console.AddCommand("kick", "i?r", "Kick player with specified id for any reason", ConfigFlags.Server, ConsoleKick);
            Console.AddCommand("record", "?s", "Record to a file", ConfigFlags.Server | ConfigFlags.Store, ConsoleRecord);
            Console.AddCommand("stoprecord", string.Empty, "Stop recording", ConfigFlags.Server, ConsoleStopRecord);

            GameContext.RegisterConsoleCommands();
        }

        protected virtual void ConsoleModStatus(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleModCommand(ConsoleCommandResult result, int clientId, ref object data)
        {
            var cmd = (string) result[0];
            var command = Console.FindCommand(cmd, ConfigFlags.Server);
            if (command == null)
            {
                Console.Print(OutputLevel.Standard, "console", $"No such command '{cmd}'");
                return;
            }

            if (result.NumArguments == 2)
            {
                var prevAccessLevel = command.AccessLevel;
                command.AccessLevel = (int) result[1];
                Console.Print(OutputLevel.Standard, "console",
                    $"moderator access for '{cmd}' is now '{command.AccessLevel <= BaseServerClient.AuthedModerator}'");

                if (command.AccessLevel != prevAccessLevel &&
                    command.AccessLevel <= BaseServerClient.AuthedModerator)
                {
                    for (var i = 0; i < Clients.Length; i++)
                    {
                        if (Clients[i].State == ServerClientState.Empty ||
                            Clients[i].AccessLevel != BaseServerClient.AuthedModerator || (
                                Clients[i].SendCommandsEnumerator != null &&
                                Clients[i].SendCommandsEnumerator.Current.Value.Cmd == cmd))
                        {
                            continue;
                        }

                        if (prevAccessLevel == BaseServerClient.AuthedAdmin)
                            SendRconCommandAdd(command, i);
                        else
                            SendRconCommandRemove(command, i);
                    }
                }
            }
            else
            {
                data = null;
                Console.Print(OutputLevel.Standard, "console",
                    $"moderator access for '{cmd}' is '{command.AccessLevel <= BaseServerClient.AuthedModerator}'");
            }
        }

        protected virtual void ConsoleRconPasswordUpdated(ConsoleCommandResult result, int clientId, ref object data)
        {
        }

        protected virtual void ConsoleOutputLevelUpdated(ConsoleCommandResult result, int clientId, ref object data)
        {
            if (result.NumArguments != 1)
                return;

            PrintCallbackInfo.OutputLevel = (OutputLevel) result[0];
        }

        protected virtual void ConsoleMaxClientsPerIpUpdated(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleSpecialInfoUpdated(ConsoleCommandResult result, int clientId, ref object data)
        {
            if (result.NumArguments > 0)
            {
                var configVar = ((ConfigString) Config["SvName"]);
                configVar.Value = configVar.Value.Trim();
                SendServerInfo(-1);
            }
        }

        protected virtual void ConsoleReload(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleStopRecord(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleRecord(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleLogout(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleShutdown(ConsoleCommandResult result, int clientId, ref object data)
        {
            throw new NotImplementedException();
        }

        protected virtual void ConsoleStatus(ConsoleCommandResult result, int clientId, ref object data)
        {
            for (var i = 0; i < Clients.Length; i++)
            {
                if (Clients[i].State == ServerClientState.Empty)
                    continue;

                string line;
                var endPoint = NetworkServer.ClientEndPoint(i);
                if (ClientInGame(i))
                {
                    var auth = Clients[i].AccessLevel == BaseServerClient.AuthedAdmin ? "(admin)" :
                               Clients[i].AccessLevel == BaseServerClient.AuthedModerator ? "(moderator)" : string.Empty;

                    line = $"id={i} endpoint={endPoint} client={Clients[i].Version:X} name='{ClientName(i)}' " +
                           $"score={GameContext.GameController.Score(i)} team={GameContext.Players[i].Team} {auth}";
                }
                else
                {
                    line = $"id={i} endpoint={endPoint} connecting";
                }

                Console.Print(OutputLevel.Standard, "server", line);
            }
        }

        protected virtual void ConsoleKick(ConsoleCommandResult result, int clientId, ref object data)
        {
            var kickId = (int) result[0];
            if (kickId < 0 || kickId >= Clients.Length)
            {
                SendRconLine(clientId, "Wrong kick id");
                return;
            }

            if (result.NumArguments > 1)
            {
                Kick(kickId, (string) result[1]);
            }
            else
                Kick(kickId, "Kicked by console");
        }
    }
}

using System;
using System.IO;
using System.Net;
using System.Threading;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;
using TeeSharp.Common.Storage;
using TeeSharp.Core;
using TeeSharp.MasterServer;
using TeeSharp.Network;
using TeeSharp.Network.Enums;
using TeeSharp.Server.Game;

namespace TeeSharp.Server
{
    public class DefaultServerKernel : IKernelConfig
    {
        public void Load(IKernel kernel)
        {
            // singletons
            kernel.Bind<BaseServer>().To<Server>().AsSingleton();
            kernel.Bind<BaseConfig>().To<ServerConfig>().AsSingleton();
            kernel.Bind<BaseNetworkBan>().To<NetworkBan>().AsSingleton();
            kernel.Bind<BaseGameContext>().To<GameContext>().AsSingleton();
            kernel.Bind<BaseStorage>().To<Storage>().AsSingleton();
            kernel.Bind<BaseNetworkServer>().To<NetworkServer>().AsSingleton();
            kernel.Bind<BaseGameConsole>().To<GameConsole>().AsSingleton();
            kernel.Bind<BaseRegister>().To<Register>().AsSingleton();

            kernel.Bind<BaseServerClient>().To<ServerClient>();
            kernel.Bind<BaseNetworkConnection>().To<NetworkConnection>();
            kernel.Bind<BaseChunkReceiver>().To<ChunkReceiver>();
        }
    }

    public class Server : BaseServer
    {
        public override long Tick { get; protected set; }

        protected override SnapshotBuilder SnapshotBuilder { get; set; }
        protected override BaseNetworkBan NetworkBan { get; set; }
        protected override BaseRegister Register { get; set; }
        protected override BaseGameContext GameContext { get; set; }
        protected override BaseConfig Config { get; set; }
        protected override BaseGameConsole Console { get; set; }
        protected override BaseStorage Storage { get; set; }
        protected override BaseNetworkServer NetworkServer { get; set; }

        protected override BaseServerClient[] Clients { get; set; }
        protected override long StartTime { get; set; }
        protected override bool IsRunning { get; set; }

        public override void Init(string[] args)
        {
            Tick = 0;
            StartTime = 0;
            Clients = new BaseServerClient[MAX_CLIENTS];

            for (var i = 0; i < MAX_CLIENTS; i++)
            {
                Clients[i] = Kernel.Get<BaseServerClient>();
            }

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
            Console.RegisterPrintCallback((OutputLevel) Config["ConsoleOutputLevel"].AsInt(),
                SendRconLineAuthed);

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

            if (!LoadMap(Config["SvMap"].AsString()))
            {
                Debug.Error("server", $"failed to load map. mapname='{Config["SvMap"]}'");
                return;    
            }

            var bindAddr = IPAddress.Any;
            if (!string.IsNullOrWhiteSpace(Config["Bindaddr"].AsString()))
                bindAddr = IPAddress.Parse(Config["Bindaddr"].AsString());

            var networkConfig = new NetworkServerConfig
            {
                LocalEndPoint = new IPEndPoint(bindAddr, Config["SvPort"].AsInt()),
                ConnectionTimeout = Config["ConnTimeout"].AsInt(),
                MaxClientsPerIp = Config["SvMaxClientsPerIP"].AsInt(),
                MaxClients = Config["SvMaxClients"].AsInt()
            };

            if (!NetworkServer.Open(networkConfig))
            {
                Debug.Error("server", $"couldn't open socket. port {networkConfig.LocalEndPoint.Port} might already be in use");
                return;
            }
            Debug.Log("server", $"network server running at: {networkConfig.LocalEndPoint}");

            NetworkServer.SetCallbacks(NewClientCallback, DelClientCallback);
            Console.Print(OutputLevel.STANDARD, "server", $"server name is '{Config["SvName"]}'");
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

                    for (var clientId = 0; clientId < MAX_CLIENTS; clientId++)
                    {
                        if (Clients[clientId].State != ServerClientState.IN_GAME)
                            continue;

                        for (var inputIndex = 0; inputIndex < Clients[clientId].Inputs.Length; inputIndex++)
                        {
                            if (Clients[clientId].Inputs[inputIndex].Tick == Tick)
                            {
                                GameContext.OnClientPredictedInput(clientId,
                                    Clients[clientId].Inputs[inputIndex].Data);
                            }
                        }
                    }

                    GameContext.OnTick();
                }

                if (ticks != 0)
                {
                    if (Tick % 2 == 0 || Config["SvHighBandwidth"].AsBoolean())
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

        public override bool SendMsgEx(MsgPacker msg, MsgFlags flags, int clientId, bool system)
        {
            if (msg == null)
                return false;

            var packet = new NetworkChunk()
            {
                ClientId = clientId,
                DataSize = msg.Size(),
                Data = msg.Data(),
            };

            packet.Data[0] <<= 1;
            if (system)
                packet.Data[0] |= 1;

            if (flags.HasFlag(MsgFlags.VITAL))
                packet.Flags |= SendFlags.VITAL;
            if (flags.HasFlag(MsgFlags.FLUSH))
                packet.Flags |= SendFlags.FLUSH;

            if (flags.HasFlag(MsgFlags.NOSEND))
            {
                if (clientId == -1)
                {
                    for (var i = 0; i < Clients.Length; i++)
                    {
                        if (Clients[i].State == ServerClientState.IN_GAME)
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
            }

            return true;
        }

        protected override void ProcessClientPacket(NetworkChunk packet)
        {
            var clientId = packet.ClientId;
            var unpacker = new Unpacker();
            unpacker.Reset(packet.Data, packet.DataSize);

            var msg = unpacker.GetInt();
            var sys = msg & 1;
            msg >>= 1;

            if (unpacker.Error)
                return;

            var message = (NetworkMessages) msg;
            if (Config["SvNetlimit"].AsBoolean() && message != NetworkMessages.REQUEST_MAP_DATA)
            {
                var now = Time.Get();
                var diff = now - Clients[clientId].TrafficSince;
                var alpha = Config["SvNetlimitAlpha"].AsInt() / 100f;
                var limit = (float) Config["SvNetlimit"].AsInt() * 1024 / Time.Freq();

                if (Clients[clientId].Traffic > limit)
                {
                    NetworkBan.BanAddr(packet.EndPoint, 600, "Stressing network");
                    return;
                }

                if (diff > 100)
                {
                    Clients[clientId].Traffic = (long) (alpha * ((float) packet.DataSize / diff) +
                                                       (1.0f - alpha) * Clients[clientId].Traffic);
                    Clients[clientId].TrafficSince = now;
                }
            }

            if (sys != 0)
            {
                switch (message)
                {
                    case NetworkMessages.INFO:
                        NetMsgInfo(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.REQUEST_MAP_DATA:
                        NetMsgRequestMapData(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.READY:
                        NetMsgReady(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.ENTERGAME:
                        NetMsgEnterGame(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.INPUT:
                        NetMsgInput(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.RCON_CMD:
                        NetMsgRconCmd(packet, unpacker, clientId);
                        break;
                    case NetworkMessages.RCON_AUTH:
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
            else
            {
                if (Clients[clientId].State >= ServerClientState.READY)
                {
                    GameContext.OnMessage(message, unpacker, clientId);
                }
            }
        }

        protected override void NetMsgPing(NetworkChunk packet, Unpacker unpacker, int clientId)
        {
            var msg = new MsgPacker(NetworkMessages.PING_REPLY);
            SendMsgEx(msg, 0, clientId, true);
        }

        protected override void NetMsgRconAuth(NetworkChunk packet, Unpacker unpacker, int clientId)
        {
            throw new NotImplementedException();
        }

        protected override void NetMsgRconCmd(NetworkChunk packet, Unpacker unpacker, int clientId)
        {
            throw new NotImplementedException();
        }

        protected override void NetMsgInput(NetworkChunk packet, Unpacker unpacker, int clientId)
        {
            Clients[clientId].LastAckedSnapshot = (long) unpacker.GetInt();
            var intendedTick = (long) unpacker.GetInt();
            var size = unpacker.GetInt();

            if (unpacker.Error || size / sizeof(int) > BaseServerClient.MAX_INPUT_SIZE)
                return;

            if (Clients[clientId].LastAckedSnapshot > 0)
                Clients[clientId].SnapRate = SnapRate.FULL;

            var now = Time.Get();
            // TODO
        }

        protected override void NetMsgEnterGame(NetworkChunk packet, Unpacker unpacker, int clientId)
        {
            throw new NotImplementedException();
        }

        protected override void NetMsgReady(NetworkChunk packet, Unpacker unpacker, int clientId)
        {
            throw new NotImplementedException();
        }

        protected override void NetMsgRequestMapData(NetworkChunk packet, Unpacker unpacker, int clientId)
        {
            throw new NotImplementedException();
        }

        protected override void NetMsgInfo(NetworkChunk packet, Unpacker unpacker, int clientId)
        {
            throw new NotImplementedException();
        }

        protected override void PumpNetwork()
        {
            NetworkServer.Update();

            while (NetworkServer.Receive(out var packet))
            {
                if (packet.ClientId == -1)
                {
                    if (packet.DataSize == MasterServerPackets.SERVERBROWSE_GETINFO.Length + 1 &&
                        packet.Data.ArrayCompare(
                            MasterServerPackets.SERVERBROWSE_GETINFO,
                            MasterServerPackets.SERVERBROWSE_GETINFO.Length))
                    {
                        SendServerInfo(
                            packet.EndPoint, 
                            packet.Data[MasterServerPackets.SERVERBROWSE_GETINFO.Length],
                            false
                        );
                    }
                    else if (packet.DataSize == MasterServerPackets.SERVERBROWSE_GETINFO64.Length + 1 &&
                             packet.Data.ArrayCompare(
                                 MasterServerPackets.SERVERBROWSE_GETINFO64,
                                 MasterServerPackets.SERVERBROWSE_GETINFO64.Length))
                    {
                        SendServerInfo(
                            packet.EndPoint,
                            packet.Data[MasterServerPackets.SERVERBROWSE_GETINFO64.Length],
                            true
                        );
                    }

                    continue;
                }

                ProcessClientPacket(packet);
            }

            NetworkBan.Update();
        }

        protected override void DoSnapshot()
        {
            GameContext.OnBeforeSnapshot();

            for (var i = 0; i < Clients.Length; i++)
            {
                if (Clients[i].State != ServerClientState.IN_GAME ||
                    Clients[i].SnapRate == SnapRate.INIT && Tick % 10 != 0 ||
                    Clients[i].SnapRate == SnapRate.RECOVER && Tick % SERVER_TICK_SPEED != 0)
                {
                    continue;
                }

                SnapshotBuilder.StartBuild();
                GameContext.OnSnapshot(i);
                var now = Time.Get();
                var snapshot =  SnapshotBuilder.EndBuild();
                var CRC = snapshot.Crc();
                
                Clients[i].SnapshotStorage.PurgeUntil(Tick - SERVER_TICK_SPEED * 3);
                Clients[i].SnapshotStorage.Add(Tick, now, snapshot);

                var deltaTick = -1L;

                if (Clients[i].SnapshotStorage.Get(Clients[i].LastAckedSnapshot,
                    out var _, out var deltaSnapshot))
                {
                    deltaTick = Clients[i].LastAckedSnapshot;
                }
                else
                {
                    deltaSnapshot = new Snapshot();
                    if (Clients[i].SnapRate == SnapRate.FULL)
                        Clients[i].SnapRate = SnapRate.RECOVER;
                }

                var deltaData = new int[SnapshotBuilder.MAX_SNAPSHOT_SIZE / sizeof(int)];
                var deltaSize = SnapshotDelta.CreateDelta(deltaSnapshot, snapshot, deltaData);

                if (deltaSize == 0)
                {
                    var msg = new MsgPacker(NetworkMessages.SNAPEMPTY);
                    msg.AddInt((int) Tick);
                    msg.AddInt((int) (Tick - deltaTick));
                    SendMsgEx(msg, MsgFlags.FLUSH, i, true);
                    continue;
                }

                var snapData = new byte[SnapshotBuilder.MAX_SNAPSHOT_SIZE];
                var snapshotSize = IntCompression.Compress(deltaData, 0, deltaSize, snapData, 0);
                var numPackets = (snapshotSize + MAX_SNAPSHOT_PACKSIZE - 1) / MAX_SNAPSHOT_PACKSIZE;

                for (int n = 0, left = snapshotSize; left != 0; n++)
                {
                    var chunk = left < MAX_SNAPSHOT_PACKSIZE ? left : MAX_SNAPSHOT_PACKSIZE;
                    left -= chunk;

                    if (numPackets == 1)
                    {
                        var msg = new MsgPacker(NetworkMessages.SNAPSINGLE);
                        msg.AddInt((int) Tick);
                        msg.AddInt((int) (Tick - deltaTick));
                        msg.AddInt(CRC);
                        msg.AddInt(chunk);
                        msg.AddRaw(snapData, n * MAX_SNAPSHOT_PACKSIZE, chunk);
                        SendMsgEx(msg, MsgFlags.FLUSH, i, true);
                    }
                    else
                    {
                        var msg = new MsgPacker(NetworkMessages.SNAP);
                        msg.AddInt((int) Tick);
                        msg.AddInt((int)(Tick - deltaTick));
                        msg.AddInt(numPackets);
                        msg.AddInt(n);
                        msg.AddInt(CRC);
                        msg.AddInt(chunk);
                        msg.AddRaw(snapData, n * MAX_SNAPSHOT_PACKSIZE, chunk);
                        SendMsgEx(msg, MsgFlags.FLUSH, i, true);
                    }
                }
            }

            GameContext.OnAfterSnapshot();
        }

        protected override long TickStartTime(long tick)
        {
            return StartTime + (Time.Freq() * tick) / SERVER_TICK_SPEED;

        }

        protected override void DelClientCallback(int clientid, string reason)
        {
            throw new NotImplementedException();
        }

        protected override void NewClientCallback(int clientid)
        {
            Clients[clientid].State = ServerClientState.AUTH;
            Clients[clientid].PlayerName = string.Empty;
            Clients[clientid].PlayerClan = string.Empty;
            Clients[clientid].PlayerCountry = -1;

            Clients[clientid].Reset();
        }

        protected override string MapName()
        {
            return Path.GetFileNameWithoutExtension(Config["SvMap"].AsString());
        }

        protected override bool LoadMap(string mapName)
        {
            return true;
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

                    if (GameContext.IsClientInGame(i))
                        playersCount++;
                    clientsCount++;
                }

                if (showMore)
                {
                    packer.AddRaw(MasterServerPackets.SERVERBROWSE_INFO64, 0, MasterServerPackets.SERVERBROWSE_INFO64.Length);
                }
                else
                {
                    packer.AddRaw(MasterServerPackets.SERVERBROWSE_INFO, 0, MasterServerPackets.SERVERBROWSE_INFO.Length);
                }

                packer.AddString(token.ToString(), 6);
                packer.AddString(GameContext.GameVersion, 32);

                if (showMore)
                {
                    packer.AddString(Config["SvName"].AsString(), 256);
                }
                else
                {
                    packer.AddString(maxClients <= VANILLA_MAX_CLIENTS
                        ? Config["SvName"].AsString()
                        : $"{Config["SvName"].AsString()} [{clientsCount}/{maxClients}]", 64);
                }

                packer.AddString(MapName(), 32);
                packer.AddString(GameContext.GameController.GameType, 16);
                packer.AddString(string.IsNullOrWhiteSpace(Config["Password"].AsString())
                    ? "0"
                    : "1", 2);

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
                packer.AddString((maxClients - Config["SvSpectatorSlots"].AsInt()).ToString(), 3);
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
                    packer.AddString(GameContext.IsClientInGame(i)
                        ? "1"
                        : "0", 2);
                }

                NetworkServer.Send(new NetworkChunk
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

        protected override string GetClientName(int clientId)
        {
            throw new NotImplementedException();
        }

        protected override string GetClientClan(int clientId)
        {
            throw new NotImplementedException();
        }

        protected override int GetClientCountry(int clientId)
        {
            throw new NotImplementedException();
        }

        protected override int GetClientScore(int clientId)
        {
            throw new NotImplementedException();
        }

        protected override void SendRconLineAuthed(string message, object data)
        {
            
        }
    }
}

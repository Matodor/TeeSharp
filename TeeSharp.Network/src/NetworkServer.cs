using System.Net;
using System.Net.Sockets;
using TeeSharp.Core;
using TeeSharp.Network.Enums;
using Math = System.Math;

namespace TeeSharp.Network
{
    public class NetworkServer : BaseNetworkServer
    {
        public override NetworkServerConfig Config { get; protected set; }

        protected override BaseChunkReceiver ChunkReceiver { get; set; }
        protected BaseNetworkBan NetworkBan { get; private set; }
        
        protected override UdpClient UdpClient { get; set; }
        protected override NewClientCallback NewClientCallback { get; set; }
        protected override DelClientCallback DelClientCallback { get; set; }
        protected override BaseNetworkConnection[] Connections { get; set; }

        public override void Init()
        {
            NetworkBan = Kernel.Get<BaseNetworkBan>();
            ChunkReceiver = Kernel.Get<BaseChunkReceiver>();
        }

        public override bool Open(NetworkServerConfig config)
        {
            if (!NetworkCore.CreateUdpClient(config.LocalEndPoint, out var socket))
                return false;

            Config = CheckConfig(config);
            UdpClient = socket;
            Connections = new BaseNetworkConnection[Config.MaxClients];

            for (var i = 0; i < Connections.Length; i++)
            {
                Connections[i] = Kernel.Get<BaseNetworkConnection>();
                Connections[i].Init(UdpClient, new NetworkConnectionConfig
                {
                    ConnectionTimeout = Config.ConnectionTimeout
                });
            }

            return true;
        }

        public override void SetCallbacks(NewClientCallback newClientCB, DelClientCallback delClientCB)
        {
            NewClientCallback = newClientCB;
            DelClientCallback = delClientCB;
        }

        public override IPEndPoint ClientEndPoint(int clientId)
        {
            return Connections[clientId].EndPoint;
        }

        public override AddressFamily NetType()
        {
            return UdpClient.Client.AddressFamily;
        }

        public override void Update()
        {
            var now = Time.Get();

            for (var clientId = 0; clientId < Connections.Length; clientId++)
            {
                Connections[clientId].Update();

                if (Connections[clientId].State == ConnectionState.ERROR)
                {
                    if (now - Connections[clientId].ConnectedAt < Time.Freq())
                        NetworkBan.BanAddr(ClientEndPoint(clientId), 60, "Stressing network");
                    else
                        Drop(clientId, Connections[clientId].Error);
                }
            }
        }

        public override bool Receive(out NetworkChunk packet)
        {
            while (UdpClient.Available > 0)
            {
                if (ChunkReceiver.FetchChunk(out packet))
                    return true;

                var remote = (IPEndPoint) null;
                var data = UdpClient.Receive(ref remote);

                if (data.Length == 0)
                    continue;

                if (NetworkBan.IsBanned(remote, out var reason))
                {
                    NetworkCore.SendControlMsg(UdpClient, remote, 0,
                        ConnectionMessages.CLOSE, reason);
                    return false;
                }

                if (!NetworkCore.UnpackPacket(data, data.Length, ChunkReceiver.ChunkConstruct))
                    return false;

                if (ChunkReceiver.ChunkConstruct.Flags.HasFlag(PacketFlags.CONNLESS))
                {
                    packet = new NetworkChunk
                    {
                        ClientId = -1,
                        Flags = SendFlags.CONNLESS,
                        EndPoint = remote,
                        DataSize = ChunkReceiver.ChunkConstruct.DataSize,
                        Data = ChunkReceiver.ChunkConstruct.Data
                    };

                    return true;
                }

                if (ChunkReceiver.ChunkConstruct.Flags.HasFlag(PacketFlags.CONTROL) &&
                    ChunkReceiver.ChunkConstruct.DataSize == 0)
                {
                    continue;
                }

                var clientId = FindSlot(remote, true);

                if (clientId < 0 &&
                    ChunkReceiver.ChunkConstruct.Flags.HasFlag(PacketFlags.CONTROL) &&
                    ChunkReceiver.ChunkConstruct.Data[0] == (int) ConnectionMessages.CONNECT)
                {
                    var sameIps = 0;
                    var freeSlotId = -1;

                    for (var i = 0; i < Connections.Length; i++)
                    {
                        if (Connections[i].State == ConnectionState.OFFLINE)
                        {
                            if (freeSlotId < 0)
                                freeSlotId = i;
                            continue;
                        }

                        if (!NetworkCore.CompareEndPoints(Connections[i].EndPoint, remote, false))
                            continue;

                        sameIps++;
                        if (sameIps >= Config.MaxClientsPerIp)
                        {
                            NetworkCore.SendControlMsg(UdpClient, remote, 0, ConnectionMessages.CLOSE,
                                $"Only {Config.MaxClientsPerIp} players with the same IP are allowed");
                            return false;
                        }
                    }

                    if (freeSlotId < 0)
                    {
                        for (var i = 0; i < Connections.Length; i++)
                        {
                            if (Connections[i].State == ConnectionState.OFFLINE)
                            {
                                freeSlotId = i;
                                break;
                            }
                        }
                    }

                    if (freeSlotId < 0)
                    {
                        NetworkCore.SendControlMsg(UdpClient, remote, 0, ConnectionMessages.CLOSE,
                            "This server is full");
                    }
                    else
                    {
                        Connections[freeSlotId].Feed(ChunkReceiver.ChunkConstruct, remote);
                        NewClientCallback?.Invoke(freeSlotId);
                        return false;
                    }
                }
                else
                {
                    if (!Connections[clientId].Feed(ChunkReceiver.ChunkConstruct, remote))
                        continue;

                    if (ChunkReceiver.ChunkConstruct.DataSize > 0)
                        ChunkReceiver.Start(remote, Connections[clientId], clientId);
                }

            }

            packet = null;
            return false;
        }

        public override void Drop(int clientId, string reason)
        {
            DelClientCallback?.Invoke(clientId, reason);
            Connections[clientId].Disconnect(reason);
        }

        public override int FindSlot(IPEndPoint endPoint, bool comparePorts)
        {
            for (var i = 0; i < Connections.Length; i++)
            {
                if (Connections[i].State != ConnectionState.OFFLINE &&
                    Connections[i].State != ConnectionState.ERROR &&
                    NetworkCore.CompareEndPoints(Connections[i].EndPoint, endPoint, true))
                {
                    return i;
                }
            }

            return -1;
        }

        public override void Send(NetworkChunk packet)
        {
            if (packet.DataSize > NetworkCore.MAX_PAYLOAD)
            {
                Debug.Warning("network", $"packet payload too big, length={packet.DataSize}");
                return;
            }

            if (packet.Flags.HasFlag(PacketFlags.CONNLESS))
            {
                NetworkCore.SendPacketConnless(UdpClient, packet.EndPoint,
                    packet.Data, packet.DataSize);
                return;
            }

            Debug.Assert(packet.ClientId >= 0 || 
                         packet.ClientId < Connections.Length, "wrong client id");

            var flags = ChunkFlags.NONE;
            if (packet.Flags.HasFlag(SendFlags.VITAL))
                flags = ChunkFlags.VITAL;

            if (Connections[packet.ClientId].QueueChunk(flags, packet.Data, packet.DataSize))
            {
                if (packet.Flags.HasFlag(SendFlags.FLUSH))
                    Connections[packet.ClientId].Flush();
            }
            else
            {
                Drop(packet.ClientId, "Error sending data");
            }
        }

        protected override NetworkServerConfig CheckConfig(NetworkServerConfig config)
        {
            config.MaxClientsPerIp = Math.Clamp(config.MaxClientsPerIp, 1, config.MaxClients);
            return config;
        }
    }
}
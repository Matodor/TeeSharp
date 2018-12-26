using System.Net;
using TeeSharp.Core;
using TeeSharp.Network.Enums;
using TeeSharp.Network.Extensions;
using Math = System.Math;

namespace TeeSharp.Network
{
    public class NetworkServer : BaseNetworkServer
    {
        public override void Init()
        {
            TokenManager = Kernel.Get<BaseTokenManager>();
            TokenCache = Kernel.Get<BaseTokenCache>();

            NetworkBan = Kernel.Get<BaseNetworkBan>();
            ChunkReceiver = Kernel.Get<BaseChunkReceiver>();
        }

        public override bool Open(NetworkServerConfig config)
        {
            if (!NetworkHelper.UdpClient(config.BindEndPoint, out var socket))
                return false;

            UdpClient = socket;
            TokenManager.Init(UdpClient);
            TokenCache.Init(UdpClient, TokenManager);

            Config = CheckConfig(config);
            Connections = new BaseNetworkConnection[Config.MaxClients];

            for (var i = 0; i < Connections.Count; i++)
            {
                Connections[i] = Kernel.Get<BaseNetworkConnection>();
                Connections[i].Init(UdpClient, Config.ConnectionConfig);
            }

            return true;
        }

        public override void SetCallbacks(NewClientCallback newClientCB, DelClientCallback delClientCB)
        {
            ClientConnected = newClientCB;
            ClientDisconnected = delClientCB;
        }

        public override void Drop(int clientId, string reason)
        {
            ClientDisconnected?.Invoke(clientId, reason);
            Connections[clientId].Disconnect(reason);
        }

        public override void Update()
        {
            for (var i = 0; i < Connections.Count; i++)
            {
                Connections[i].Update();

                if (Connections[i].State == ConnectionState.Error)
                {
                    if (Time.Get() - Connections[i].ConnectedAt < Time.Freq())
                        NetworkBan.BanAddr(Connections[i].EndPoint, 60, "Stressing network");
                    else
                        Drop(i, Connections[i].Error);
                }
            }

            TokenManager.Update();
            TokenCache.Update();
        }

        public override bool Receive(ref Chunk packet, ref uint responseToken)
        {
            // TODO make multithreaded 
            // https://docs.microsoft.com/ru-ru/dotnet/standard/io/how-to-use-named-pipes-for-network-interprocess-communication
            // https://blogs.msdn.microsoft.com/dotnet/2018/07/09/system-io-pipelines-high-performance-io-in-net/

            while (true)
            {
                if (ChunkReceiver.FetchChunk(ref packet))
                    return true;

                if (UdpClient.Available <= 0)
                    return false;

                IPEndPoint endPoint = null;
                byte[] data;

                try
                {
                    data = UdpClient.Receive(ref endPoint);
                }
                catch
                {
                    continue;
                }

                if (data.Length == 0)
                    continue;

                if (!NetworkHelper.UnpackPacket(data, data.Length, ChunkReceiver.ChunkConstruct))
                    continue;

                if (NetworkBan.IsBanned(endPoint, out var banReason))
                {
                    NetworkHelper.SendConnectionMsg(UdpClient, endPoint,
                        ChunkReceiver.ChunkConstruct.ResponseToken, 0, ConnectionMessages.Close, banReason);
                    continue;
                }

                var freeSlot = -1;
                var foundSlot = -1;
                var sameIps = 0;

                for (var i = 0; i < Connections.Count; i++)
                {
                    if (
                        Connections[i].State != ConnectionState.Offline &&
                        Connections[i].EndPoint.Compare(endPoint, comparePorts: false))
                    {
                        sameIps++;

                        if (Connections[i].EndPoint.Port == endPoint.Port)
                        {
                            foundSlot = i;

                            if (Connections[i].Feed(ChunkReceiver.ChunkConstruct, endPoint))
                            {
                                if (!ChunkReceiver.ChunkConstruct.Flags.HasFlag(PacketFlags.Connless))
                                    ChunkReceiver.Start(endPoint, Connections[i], i);
                                else
                                {
                                    packet = new Chunk()
                                    {
                                        Flags = SendFlags.Connless,
                                        EndPoint = endPoint,
                                        ClientId = i,
                                        DataSize = ChunkReceiver.ChunkConstruct.DataSize,
                                        Data = ChunkReceiver.ChunkConstruct.Data
                                    };

                                    responseToken = TokenHelper.TokenNone;
                                    return true;
                                }
                            }
                        }
                    }

                    if (Connections[i].State == ConnectionState.Offline && freeSlot < 0)
                        freeSlot = i;
                }

                if (foundSlot >= 0)
                    continue;

                var accept = TokenManager.ProcessMessage(endPoint, ChunkReceiver.ChunkConstruct);
                if (accept <= 0)
                    continue;

                if (ChunkReceiver.ChunkConstruct.Flags.HasFlag(PacketFlags.Control))
                {
                    if (ChunkReceiver.ChunkConstruct.Data[0] == (int) ConnectionMessages.Connect)
                    {
                        if (sameIps >= Config.MaxClientsPerIp)
                        {
                            NetworkHelper.SendConnectionMsg(UdpClient, endPoint, 
                                ChunkReceiver.ChunkConstruct.ResponseToken, 0, 
                                ConnectionMessages.Close, $"Only {Config.MaxClientsPerIp} players with the same IP are allowed");
                            return false;
                        }

                        if (freeSlot >= 0)
                        {
                            Connections[freeSlot].SetToken(ChunkReceiver.ChunkConstruct.Token);
                            Connections[freeSlot].Feed(ChunkReceiver.ChunkConstruct, endPoint);
                            ClientConnected?.Invoke(freeSlot);
                            return false;
                        }
                        
                        NetworkHelper.SendConnectionMsg(UdpClient, endPoint,
                            ChunkReceiver.ChunkConstruct.ResponseToken, 0,
                            ConnectionMessages.Close, "This server is full");
                        return false;
                    }

                    if (ChunkReceiver.ChunkConstruct.Data[0] == (int) ConnectionMessages.Token)
                    {
                        TokenCache.AddToken(endPoint, ChunkReceiver.ChunkConstruct.ResponseToken,
                            TokenFlags.ResponseOnly);
                    }
                }
                else if (ChunkReceiver.ChunkConstruct.Flags.HasFlag(PacketFlags.Connless))
                {
                    packet = new Chunk()
                    {
                        Flags = SendFlags.Connless,
                        ClientId = -1,
                        EndPoint = endPoint,
                        DataSize = ChunkReceiver.ChunkConstruct.DataSize,
                        Data = ChunkReceiver.ChunkConstruct.Data,
                    };

                    responseToken = ChunkReceiver.ChunkConstruct.ResponseToken;
                    return true;
                }
            }
        }

        public override bool Send(Chunk packet, uint token = TokenHelper.TokenNone)
        {
            if (packet.Flags.HasFlag(SendFlags.Connless))
            {
                if (packet.DataSize >= NetworkHelper.MaxPayload)
                {
                    Debug.Warning("netserver", $"packet payload too big ({packet.DataSize}). dropping packet");
                    return false;
                }

                if (packet.ClientId == -1)
                {
                    for (var i = 0; i < Connections.Count; i++)
                    {
                        if (Connections[i].State != ConnectionState.Offline && 
                            Connections[i].EndPoint != null &&
                            Connections[i].EndPoint.Compare(packet.EndPoint, comparePorts: true))
                        {
                            packet.ClientId = i;
                            break;
                        }
                    }
                }

                if (token != TokenHelper.TokenNone)
                {
                    NetworkHelper.SendPacketConnless(UdpClient, packet.EndPoint, token,
                        TokenManager.GenerateToken(packet.EndPoint), packet.Data, packet.DataSize);
                }
                else
                {
                    if (packet.ClientId == -1) 
                        TokenCache.SendPacketConnless(packet.EndPoint, packet.Data, packet.DataSize);
                    else
                    {
                        Debug.Assert(packet.ClientId >= 0 && packet.ClientId < Connections.Count, 
                            "errornous client id");
                        Connections[packet.ClientId].SendPacketConnless(packet.Data, packet.DataSize);
                    }
                }
            }
            else
            {
                if (packet.DataSize + NetworkHelper.MaxChunkHeaderSize >= NetworkHelper.MaxPayload)
                {
                    Debug.Warning("netserver", $"packet payload too big ({packet.DataSize}). dropping packet");
                    return false;
                }

                Debug.Assert(packet.ClientId >= 0 && packet.ClientId < Connections.Count,
                    "errornous client id");

                var flags = packet.Flags.HasFlag(SendFlags.Vital)
                    ? ChunkFlags.Vital
                    : ChunkFlags.None;

                if (Connections[packet.ClientId].QueueChunk(flags, packet.Data, packet.DataSize))
                {
                    if (packet.Flags.HasFlag(SendFlags.Flush))
                        Connections[packet.ClientId].Flush();
                }
                else
                {
                    Drop(packet.ClientId, "Error sending data");
                }
            }

            return true;
        }

        public override IPEndPoint ClientEndPoint(int clientId)
        {
            return Connections[clientId].EndPoint;
        }

        protected override NetworkServerConfig CheckConfig(NetworkServerConfig config)
        {
            return config;
        }

        public override void SetMaxClientsPerIp(int max)
        {
            var config = Config;
            config.MaxClientsPerIp = Math.Clamp(max, 1, config.MaxClients);
            Config = config;
        }

        public override void AddToken(IPEndPoint endPoint, uint token)
        {
            TokenCache.AddToken(endPoint, token, TokenFlags.None);
        }
    }
}
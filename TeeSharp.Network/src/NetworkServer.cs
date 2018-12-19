using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Enums;
using TeeSharp.Core;
using TeeSharp.Network.Enums;
using Math = System.Math;

namespace TeeSharp.Network
{
    public class NetworkServer : BaseNetworkServer
    {
        public override NetworkServerConfig ServerConfig { get; protected set; }

        protected override BaseChunkReceiver ChunkReceiver { get; set; }
        protected override BaseNetworkBan NetworkBan { get; set; }
        protected override BaseConfig Config { get; set; }
        
        protected override UdpClient UdpClient { get; set; }
        protected override NewClientCallback NewClientCallback { get; set; }
        protected override DelClientCallback DelClientCallback { get; set; }
        protected override BaseNetworkConnection[] Connections { get; set; }
        
        protected virtual int CurrentSalt { get; set; }
        protected virtual byte[][] Salts { get; set; }
        protected virtual long LastSaltUpdate { get; set; }
        protected virtual long LegacyRateLimitStart { get; set; }
        protected virtual int LegacyRateLimitNum { get; set; }

        public override bool Open(NetworkServerConfig config)
        {
            if (!NetworkCore.CreateUdpClient(config.LocalEndPoint, out var socket))
                return false;

            ServerConfig = CheckConfig(config);
            UdpClient = socket;
            Connections = new BaseNetworkConnection[ServerConfig.MaxClients];

            for (var i = 0; i < Salts.Length; i++)
                Secure.RandomFill(Salts[i]);

            LastSaltUpdate = Time.Get();
            LegacyRateLimitStart = -1;
            LegacyRateLimitNum = 0;

            for (var i = 0; i < Connections.Length; i++)
            {
                Connections[i] = Kernel.Get<BaseNetworkConnection>();
                Connections[i].Init(UdpClient);
            }

            return true;
        }

        public override void SetCallbacks(NewClientCallback newClientCB, DelClientCallback delClientCB)
        {
            NewClientCallback = newClientCB;
            DelClientCallback = delClientCB;
        }

        public override void Drop(int clientId, string reason)
        {
            DelClientCallback?.Invoke(clientId, reason);
            Connections[clientId].Disconnect(reason);
        }

        public override void Update()
        {
            var timeNow = Time.Get();

            if (timeNow >= LastSaltUpdate + 10 * Time.Freq())
            {
                CurrentSalt = (CurrentSalt + 1) % Salts.Length;
                Secure.RandomFill(Salts[CurrentSalt]);
                LastSaltUpdate = timeNow;
            }

            for (var clientId = 0; clientId < Connections.Length; clientId++)
            {
                Connections[clientId].Update();

                if (Connections[clientId].State == ConnectionState.ERROR)
                {
                    if (Time.Get() - Connections[clientId].ConnectedAt < Time.Freq())
                        NetworkBan.BanAddr(ClientEndPoint(clientId), 60, "Stressing network");
                    else
                        Drop(clientId, Connections[clientId].Error);
                }
            }
        }
        
        protected virtual uint GetToken(IPEndPoint addr)
        {
            return GetToken(addr, CurrentSalt);
        }

        protected virtual uint GetToken(IPEndPoint addr, int saltIndex)
        {
            using (var stream = new MemoryStream())
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    stream.Write(BitConverter.GetBytes(1), 0, 4);
                    stream.Write(addr.Address.GetAddressBytes(), 0, 4);
                    stream.Write(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 0, 12);
                    stream.Write(BitConverter.GetBytes(addr.Port), 0, 4);
                }

                stream.Write(Salts[CurrentSalt], 0, Salts[CurrentSalt].Length);

                return Secure.MD5.ComputeHash(stream.ToArray()).ToUInt32();
            }
        }

        protected virtual bool IsCorrectToken(IPEndPoint addr, uint token)
        {
            for (var i = 0; i < Salts.Length; i++)
            {
                if (GetToken(addr, i) == token)
                    return true;
            }

            return false;
        }

        protected virtual uint GetLegacyToken(IPEndPoint addr)
        {
            return GetLegacyToken(addr, CurrentSalt);
        }

        protected virtual uint GetLegacyToken(IPEndPoint addr, int saltIndex)
        {
            return DeriveLegacyToken(GetToken(addr, saltIndex));
        }

        public override void Init()
        {
            NetworkBan = Kernel.Get<BaseNetworkBan>();
            ChunkReceiver = Kernel.Get<BaseChunkReceiver>();
            Config = Kernel.Get<BaseConfig>();

            Salts = new byte[2][];
            for (var i = 0; i < Salts.Length; i++)
                Salts[i] = new byte[16];
        }

        protected virtual uint DeriveLegacyToken(uint token)
        {
            token &= ~0x80000000;
            if (token < 2)
                token += 2;
            return token;
        }

        protected virtual bool IsCorrectLegacyToken(IPEndPoint addr, uint legacyToken)
        {
            for (var i = 0; i < Salts.Length; i++)
            {
                if (GetLegacyToken(addr, i) == legacyToken)
                    return true;
            }

            return false;
        }

        protected virtual bool LegacyRateLimit()
        {
            var accept = false;
            var max = Config["SvOldClientsPerInterval"].AsInt();
            var interval = Config["SvOldClientsInterval"].AsInt();
            var useRateLimit = max > 0 && interval > 0;

            if (useRateLimit)
            {
                var now = Time.Get();

                if (LegacyRateLimitStart < 0 ||
                    LegacyRateLimitStart + interval * Time.Freq() <= now)
                {
                    LegacyRateLimitStart = now;
                    LegacyRateLimitNum = Math.Clamp(LegacyRateLimitNum - max, 0, max);
                }

                accept = LegacyRateLimitNum < max;
            }

            if (Config["SvOldClientsSkip"] > 0 && (!accept || !useRateLimit))
            {
                accept = new Random().Next(0, int.MaxValue) <=
                         int.MaxValue / Config["SvOldClientsSkip"];
            }

            if (accept && useRateLimit)
                LegacyRateLimitNum++;

            return !accept;
        }

        protected virtual bool DecodeLegacyHandShake(byte[] data, int dataSize, out uint legacyToken)
        {
            var unpacker = new Unpacker();
            unpacker.Reset(data, dataSize);
            var msgId = unpacker.GetInt();
            var token = unpacker.GetInt();

            if (unpacker.Error || msgId != System(NetworkMessages.CL_INPUT))
            {
                legacyToken = 0;
                return false;
            }

            legacyToken = (uint) token;
            return true;
        }
        
        public override IPEndPoint ClientEndPoint(int clientId)
        {
            return Connections[clientId].EndPoint;
        }

        public override AddressFamily NetType()
        {
            return UdpClient.Client.AddressFamily;
        }

        public override bool Receive(out NetworkChunk packet)
        {
            while (true)
            {
                if (ChunkReceiver.FetchChunk(out packet))
                    return true;

                if (UdpClient.Available <= 0)
                    return false;

                var remote = (IPEndPoint) null;
                byte[] data;

                try
                {
                    data = UdpClient.Receive(ref remote);
                }
                catch
                {
                    continue;
                }
                
                if (data.Length <= 0)
                    continue;
                
                if (!NetworkCore.UnpackPacket(data, data.Length, ChunkReceiver.ChunkConstruct))
                    continue;

                var useToken = false;
                var token = (uint) 0;

                if (ChunkReceiver.ChunkConstruct.Flags.HasFlag(PacketFlags.TOKEN))
                {
                    useToken = true;
                    token = ChunkReceiver.ChunkConstruct.Token;
                }
                else if (ChunkReceiver.ChunkConstruct.Flags.HasFlag(PacketFlags.CONTROL) &&
                         ChunkReceiver.ChunkConstruct.Data[0] == (int) ConnectionMessages.CONNECT &&
                         ChunkReceiver.ChunkConstruct.DataSize >= 1 + 512)
                {
                    useToken = true;
                    token = ChunkReceiver.ChunkConstruct.Data.ToUInt32(5);
                }

                if (NetworkBan.IsBanned(remote, out var reason))
                {
                    NetworkCore.SendControlMsg(UdpClient, remote, 0, useToken, token,
                        ConnectionMessages.CLOSE, reason);
                    continue;
                }

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

                var clientId = FindSlot(remote, true);

                if (ChunkReceiver.ChunkConstruct.Flags.HasFlag(PacketFlags.CONTROL) &&
                    ChunkReceiver.ChunkConstruct.Data[0] == (int) ConnectionMessages.CONNECT)
                {
                    if (clientId != -1)
                        continue;

                    if (ChunkReceiver.ChunkConstruct.DataSize >= 1 + 512)
                    {
                        var connectAccept = new byte[4];
                        GetToken(remote).ToByteArray(connectAccept, 0);
                        NetworkCore.SendControlMsg(UdpClient, remote, 0, true, token,
                            ConnectionMessages.CONNECTACCEPT, connectAccept);
                        Debug.Log("netserver", "got connect, sending connect+accept challenge");
                    }
                    else if (Config["SvAllowOldClients"].AsBoolean() && string.IsNullOrWhiteSpace(Config["Password"].AsString()))
                    {
                        if (LegacyRateLimit())
                        {
                            Debug.Log("netserver", "dropping legacy connect due to ratelimit");
                            continue;
                        }

                        var packets = new NetworkChunkConstruct[]
                        {
                            new NetworkChunkConstruct(), 
                            new NetworkChunkConstruct(), 
                        };
                        var legacyToken = GetLegacyToken(remote);

                        ConstructLegacyHandshake(packets[0], packets[1], legacyToken);
                        for (var i = 0; i < 2; i++)
                            NetworkCore.SendPacket(UdpClient, remote, packets[i]);
                        Debug.Log("netserver", "got legacy connect, sending legacy challenge");
                    }
                    else
                    {
                        Debug.Log("netserver", $"dropping short connect packet, size={ChunkReceiver.ChunkConstruct.DataSize}");
                    }
                }
                else
                {
                    if (clientId == -1)
                    {
                        if (!useToken || !IsCorrectToken(remote, token))
                        {
                            if (!useToken && Config["SvAllowOldClients"].AsBoolean())
                            {
                                ChunkReceiver.Start(remote, null, -1);
                                var chunk = new NetworkChunk();
                                var correct = false;

                                while (ChunkReceiver.FetchChunk(out chunk))
                                {
                                    if (DecodeLegacyHandShake(chunk.Data, chunk.DataSize, out var legacyToken))
                                    {
                                        if (IsCorrectLegacyToken(remote, legacyToken))
                                        {
                                            correct = true;
                                            break;
                                        }
                                    }
                                }

                                ChunkReceiver.Clear();

                                if (!correct)
                                    continue;
                            }
                            else
                            {
                                Debug.Log("netserver",
                                    !useToken
                                        ? "dropping packet with missing token"
                                        : $"dropping packet with invalid token, token={token}");
                                continue;
                            }
                        }

                        var sameIps = 0;

                        for (var i = 0; i < Connections.Length; i++)
                        {
                            if (Connections[i].State == ConnectionState.OFFLINE)
                            {
                                if (clientId < 0)
                                    clientId = i;
                                continue;
                            }

                            if (!NetworkCore.CompareEndPoints(Connections[i].EndPoint, remote, false))
                                continue;

                            sameIps++;
                            if (sameIps >= ServerConfig.MaxClientsPerIp)
                            {
                                NetworkCore.SendControlMsg(UdpClient, remote, 0, useToken, token, ConnectionMessages.CLOSE,
                                    $"Only {ServerConfig.MaxClientsPerIp} players with the same IP are allowed");
                                return false;
                            }
                        }

                        if (clientId < 0)
                        {
                            for (var i = 0; i < Connections.Length; i++)
                            {
                                if (Connections[i].State == ConnectionState.OFFLINE)
                                {
                                    clientId = i;
                                    break;
                                }
                            }
                        }

                        if (clientId < 0)
                        {
                            NetworkCore.SendControlMsg(UdpClient, remote, 0, useToken, token,
                                ConnectionMessages.CLOSE, "This server is full");
                            return false;
                        }

                        if (useToken)
                            Connections[clientId].Accept(remote, token);
                        else
                            Connections[clientId].AcceptLegacy(remote);

                        NewClientCallback?.Invoke(clientId, !useToken);
                        if (!useToken)
                            continue;

                        Connections[clientId].Feed(ChunkReceiver.ChunkConstruct, remote);
                    }

                    if (Connections[clientId].Feed(ChunkReceiver.ChunkConstruct, remote))
                    {
                        if (ChunkReceiver.ChunkConstruct.DataSize != 0)
                            ChunkReceiver.Start(remote, Connections[clientId], clientId);
                    }
                }
            }
        }

        protected virtual void ConstructLegacyHandshake(
            NetworkChunkConstruct packet1, 
            NetworkChunkConstruct packet2, uint legacyToken)
        {
            throw new NotImplementedException();

            packet1.Flags = PacketFlags.CONTROL;
            packet1.Ack = 0;
            packet1.NumChunks = 0;
            packet1.DataSize = 1;
            packet1.Data[0] = (byte) ConnectionMessages.CONNECTACCEPT;

            packet2.Flags = PacketFlags.None;
            packet2.Ack = 0;
            packet2.NumChunks = 0;
            packet2.DataSize = 0;

            var packer = new Packer();
            packer.Reset();
            packer.AddInt(System(NetworkMessages.SV_MAP_CHANGE));
            packer.AddString("dm1");
            //packer.AddInt(4061503086);
            packer.AddInt(5805);
            AddChunk(packet2, 1, packer.Data(), packer.Size());

            packer.Reset();
            packer.AddInt(System(NetworkMessages.SV_CON_READY));
            AddChunk(packet2, 2, packer.Data(), packer.Size());

            for (var i = -2; i <= 0; i++)
            {
                packer.Reset();
                packer.AddInt(System(NetworkMessages.SV_SNAPEMPTY));
                packer.AddInt((int) (legacyToken + i));
                packer.AddInt((int) (i == -2 ? legacyToken + i + 1 : 1));
                AddChunk(packet2, -1, packer.Data(), packer.Size());
            }
        }

        private void AddChunk(NetworkChunkConstruct packet, int sequence, 
            byte[] data, int dataSize)
        {
            Debug.Assert(packet.DataSize + NetworkCore.MAX_CHUNK_HEADER_SIZE + 
                         dataSize <= packet.Data.Length, "too much data");

            var header = new NetworkChunkHeader
            {
                Flags = sequence >= 0 ? ChunkFlags.VITAL : ChunkFlags.NONE,
                Size = dataSize,
                Sequence = sequence >= 0 ? sequence : 0
            };

            var packetDataOffset = packet.DataSize;
            var chunkStartOffset = packetDataOffset;

            packetDataOffset = header.Pack(packet.Data, packetDataOffset);
            packet.DataSize += packetDataOffset - chunkStartOffset;

            Buffer.BlockCopy(data, 0, packet.Data, packetDataOffset, dataSize);
            packet.DataSize += dataSize;
            packet.NumChunks++;
        }
        
        protected virtual int System(NetworkMessages msg)
        {
            return ((int) msg << 1) | 1;
        }
        
        public override int FindSlot(IPEndPoint endPoint, bool comparePorts)
        {
            for (var i = 0; i < Connections.Length; i++)
            {
                if (Connections[i].State != ConnectionState.OFFLINE &&
                    Connections[i].State != ConnectionState.ERROR &&
                    NetworkCore.CompareEndPoints(Connections[i].EndPoint, endPoint, comparePorts))
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

            if (packet.Flags.HasFlag(SendFlags.CONNLESS))
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

        public override void SetMaxClientsPerIp(int max)
        {
            var config = ServerConfig;
            config.MaxClientsPerIp = Math.Clamp(max, 1, config.MaxClients);
            ServerConfig = config;
        }
    }
}
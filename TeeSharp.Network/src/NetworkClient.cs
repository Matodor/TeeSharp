using System.Net;
using TeeSharp.Core;
using TeeSharp.Network.Enums;
using TeeSharp.Network.Extensions;

namespace TeeSharp.Network
{
    public class NetworkClient : BaseNetworkClient
    {
        public override void Init()
        {
            TokenManager = Kernel.Get<BaseTokenManager>();
            TokenCache = Kernel.Get<BaseTokenCache>();
            ChunkReceiver = Kernel.Get<BaseChunkReceiver>();
        }

        public override bool Open(NetworkClientConfig config)
        {
            if (!NetworkHelper.UdpClient(config.LocalEndPoint, out var socket))
                return false;

            Config = config;
            UdpClient = socket;
            Connection = Kernel.Get<BaseNetworkConnection>();
            Connection.Init(UdpClient, Config.ConnectionConfig);

            TokenManager.Init(UdpClient);
            TokenCache.Init(UdpClient, TokenManager);
            return true;
        }

        public override void Close()
        {
        }

        public override void Disconnect(string reason)
        {
            Connection.Disconnect(reason);
        }

        public override bool Connect(IPEndPoint endPoint)
        {
            return Connection.Connect(endPoint);
        }

        public override void Update()
        {
            Connection.Update();
            if (Connection.State == ConnectionState.Error)
                Disconnect(Connection.Error);

            TokenManager.Update();
            TokenCache.Update();
        }

        public override bool Receive(ref Chunk packet, ref uint responseToken)
        {
            while (true)
            {
                if (ChunkReceiver.FetchChunk(ref packet))
                    return true;

                if (UdpClient.Available <= 0)
                    return false;

                var endPoint = default(IPEndPoint);
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

                if (Connection.State != ConnectionState.Offline &&
                    Connection.State != ConnectionState.Error &&
                    Connection.EndPoint.Compare(endPoint, true))
                {
                    if (Connection.Feed(ChunkReceiver.ChunkConstruct, endPoint))
                    {
                        if (!ChunkReceiver.ChunkConstruct.Flags.HasFlag(PacketFlags.Connless))
                            ChunkReceiver.Start(endPoint, Connection, 0);
                    }
                }
                else
                {
                    var accept = TokenManager.ProcessMessage(endPoint, ChunkReceiver.ChunkConstruct);
                    if (accept == 0)
                        continue;

                    if (ChunkReceiver.ChunkConstruct.Flags.HasFlag(PacketFlags.Control))
                    {
                        if (ChunkReceiver.ChunkConstruct.Data[0] == (int) ConnectionMessages.Token)
                        {
                            TokenCache.AddToken(endPoint, ChunkReceiver.ChunkConstruct.ResponseToken,
                                TokenFlags.AllowBroadcast | TokenFlags.ResponseOnly);
                        }
                    }
                    else if (ChunkReceiver.ChunkConstruct.Flags.HasFlag(PacketFlags.Connless) &&
                             accept != -1)
                    {
                        packet = new Chunk
                        {
                            ClientId = -1,
                            Flags = SendFlags.Connless,
                            EndPoint = endPoint,
                            DataSize = ChunkReceiver.ChunkConstruct.DataSize,
                            Data = ChunkReceiver.ChunkConstruct.Data,
                        };

                        responseToken = ChunkReceiver.ChunkConstruct.ResponseToken;
                        return true;
                    }
                }
            }
        }

        public override void Send(Chunk packet, uint token = TokenHelper.TokenNone,
            SendCallbackData callbackData = null)
        {
            if (packet.Flags.HasFlag(SendFlags.Connless))
            {
                if (packet.DataSize > NetworkHelper.MaxPayload)
                {
                    Debug.Warning("network", $"packet payload too big, length={packet.DataSize}");
                    return;
                }

                if (packet.ClientId == -1 && Connection.EndPoint != null && 
                    packet.EndPoint.Compare(Connection.EndPoint, true))
                {
                    packet.ClientId = 0;
                }

                if (token != TokenHelper.TokenNone)
                {
                    NetworkHelper.SendPacketConnless(UdpClient, packet.EndPoint,
                        token, TokenManager.GenerateToken(packet.EndPoint),
                        packet.Data, packet.DataSize);
                }
                else
                {
                    if (packet.ClientId == -1)
                    {
                        TokenCache.SendPacketConnless(packet.EndPoint,
                            packet.Data, packet.DataSize, callbackData);
                    }
                    else
                    {
                        Debug.Assert(packet.ClientId == 0, "errornous client id");
                        Connection.SendPacketConnless(packet.Data, packet.DataSize);
                    }
                }
            }
            else
            {
                if (packet.DataSize + NetworkHelper.MaxChunkHeaderSize >= NetworkHelper.MaxPayload)
                {
                    Debug.Warning("network", $"chunk payload too big, length={packet.DataSize} dropping chunk");
                    return;
                }

                Debug.Assert(packet.ClientId == 0, "errornous client id");

                var flags = ChunkFlags.None;
                if (packet.Flags.HasFlag(SendFlags.Vital))
                    flags = ChunkFlags.Vital;

                Connection.QueueChunk(flags, packet.Data, packet.DataSize);

                if (packet.Flags.HasFlag(SendFlags.Flush))
                    Connection.Flush();
            }
        }

        public override void PurgeStoredPacket(int trackId)
        {
            TokenCache.PurgeStoredPacket(trackId);
        }

        public override void Flush()
        {
            Connection.Flush();
        }

        public override bool GotProblems()
        {
            return Time.Get() - Connection.LastReceiveTime > Time.Freq();
        }
    }
}
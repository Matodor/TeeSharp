using System.Net;
using System.Net.Sockets;
using TeeSharp.Core;
using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public class NetworkClient : BaseNetworkClient
    {
        public override BaseNetworkConnection Connection { get; protected set; }
        public override UdpClient UdpClient { get; protected set; }

        protected override BaseChunkReceiver ChunkReceiver { get; set; }

        public override void Init()
        {
            ChunkReceiver = Kernel.Get<BaseChunkReceiver>();
        }

        public override bool Open(NetworkClientConfig config)
        {
            if (!NetworkCore.CreateUdpClient(config.LocalEndPoint, out var socket))
                return false;

            UdpClient = socket;
            Connection = Kernel.Get<BaseNetworkConnection>();
            Connection.Init(UdpClient);

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
            if (Connection.State == ConnectionState.ERROR)
                Disconnect(Connection.Error);
        }

        public override bool Receive(out NetworkChunk packet)
        {
            while (true)
            {
                if (ChunkReceiver.FetchChunk(ref packet))
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

                if (data.Length == 0)
                    continue;

                if (!NetworkCore.UnpackPacket(data, data.Length, ChunkReceiver.ChunkConstruct))
                    continue;

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

                if (Connection.Feed(ChunkReceiver.ChunkConstruct, remote))
                    ChunkReceiver.Start(remote, Connection, 0);
            }
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

            Debug.Assert(packet.ClientId == 0, "wrong client id");

            var flags = ChunkFlags.NONE;
            if (packet.Flags.HasFlag(SendFlags.VITAL))
                flags = ChunkFlags.VITAL;

            Connection.QueueChunk(flags, packet.Data, packet.DataSize);

            if (packet.Flags.HasFlag(SendFlags.FLUSH))
                Connection.Flush();
        }

        public override void Flush()
        {
            Connection.Flush();
        }

        public override bool GotProblems()
        {
            if (Time.Get() - Connection.LastReceiveTime > Time.Freq())
                return true;
            return false;
        }
    }
}
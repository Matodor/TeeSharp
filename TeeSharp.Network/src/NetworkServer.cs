using System;
using System.Net;
using TeeSharp.Core.MinIoC;

namespace TeeSharp.Network
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class NetworkServer : BaseNetworkServer
    {
        public override void Init()
        {
            ChunkFactory = Container.Resolve<BaseChunkFactory>();
            ChunkFactory.Init();
        }

        public override void Update()
        {
            
        }

        // ReSharper disable once InconsistentNaming
        public override bool Open(IPEndPoint localEP)
        {
            if (localEP == null)
                throw new ArgumentNullException(nameof(localEP));
            
            if (!NetworkBase.TryGetUdpClient(localEP, out var socket))
                return false;

            Socket = socket;
            Socket.Client.Blocking = true;
            
            return true;
        }

        public override bool Receive(out NetworkMessage netMsg, ref SecurityToken responseToken)
        {
            while (true)
            {
                if (ChunkFactory.TryGet(out netMsg))
                    return true;

                // ReSharper disable once InconsistentNaming
                var endPoint = default(IPEndPoint);
                var data = Socket.Receive(ref endPoint).AsSpan();

                if (data.Length == 0)
                    continue;

                // TODO
                // Check for banned IP address

                var isSixUp = false;
                var securityToken = default(SecurityToken);

                responseToken = SecurityToken.TokenUnknown;

                if (!NetworkBase.TryUnpackPacket(
                    data,
                    ChunkFactory.Chunks,
                    ref isSixUp,
                    ref securityToken,
                    ref responseToken))
                {
                    continue;
                }

                if (ChunkFactory.Chunks.Flags.HasFlag(ChunkFlags.ConnectionLess))
                {
                    if (isSixUp && securityToken != GetToken(endPoint))
                        continue;

                    netMsg = new NetworkMessage
                    {
                        ClientId = -1,
                        EndPoint = endPoint,
                        Flags = MessageFlags.ConnectionLess,
                        DataSize = ChunkFactory.Chunks.DataSize,
                        Data = ChunkFactory.Chunks.Data,
                    };

                    if (ChunkFactory.Chunks.Flags.HasFlag(ChunkFlags.Extended))
                    {
                        netMsg.Flags |= MessageFlags.Extended;
                        netMsg.ExtraData = new byte[NetworkConstants.ExtraDataSize];
                        ChunkFactory.Chunks.ExtraData.AsSpan().CopyTo(netMsg.ExtraData);
                    }

                    return true;

                    // 192.168.66.1:13032
                    // 138, 153, 106, 214, 139, 34, 32, 162, 24, 139, 49, 155, 18, 192, 231, 110
                }
            }

            return true;
        }

        public override SecurityToken GetToken(IPEndPoint endPoint)
        {
            throw new NotImplementedException();
        }
    }
}
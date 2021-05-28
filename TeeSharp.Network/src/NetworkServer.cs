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
            
            if (!NetworkHelper.TryGetUdpClient(localEP, out var socket))
                return false;

            Socket = socket;
            Socket.Client.Blocking = true;
            
            return true;
        }

        public override bool Receive(out NetworkMessage netMsg, ref SecurityToken responseToken)
        {
            if (ChunkFactory.TryGetMessage(out netMsg))
                return true;

            // ReSharper disable once InconsistentNaming
            var endPoint = default(IPEndPoint);
            var data = Socket.Receive(ref endPoint).AsSpan();

            if (data.Length == 0)
                return false;

            // TODO
            // Check for banned IP address

            var isSixUp = false;
            var securityToken = default(SecurityToken);

            responseToken = SecurityToken.TokenUnknown;

            if (!NetworkHelper.TryUnpackPacket(
                data,
                ChunkFactory.ChunksData,
                ref isSixUp,
                ref securityToken,
                ref responseToken))
            {
                return false;
            }

            if (ChunkFactory.ChunksData.Flags.HasFlag(ChunkFlags.ConnectionLess))
            {
                if (isSixUp && securityToken != GetToken(endPoint))
                    return false;

                netMsg = new NetworkMessage
                {
                    ClientId = -1,
                    EndPoint = endPoint,
                    Flags = MessageFlags.ConnectionLess,
                    Data = new byte[ChunkFactory.ChunksData.DataSize],
                    ExtraData = null,
                };

                ChunkFactory.ChunksData.Data
                    .AsSpan()
                    .Slice(0, ChunkFactory.ChunksData.DataSize)
                    .CopyTo(netMsg.Data);

                if (ChunkFactory.ChunksData.Flags.HasFlag(ChunkFlags.Extended))
                {
                    netMsg.Flags |= MessageFlags.Extended;
                    netMsg.ExtraData = new byte[NetworkConstants.ExtraDataSize];
                    
                    ChunkFactory.ChunksData.ExtraData
                        .AsSpan()
                        .CopyTo(netMsg.ExtraData);
                }

                return true;
            }

            if (ChunkFactory.ChunksData.Flags.HasFlag(ChunkFlags.Control) &&
                ChunkFactory.ChunksData.DataSize == 0)
            {
                return false;
            }

            return true;
        }

        public override SecurityToken GetToken(IPEndPoint endPoint)
        {
            throw new NotImplementedException();
        }
    }
}
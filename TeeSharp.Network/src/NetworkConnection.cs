using System.Net;
using System.Net.Sockets;
using TeeSharp.Core;
using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public class NetworkConnection : BaseNetworkConnection
    {
        public override ConnectionState State { get; protected set; }
        public override long ConnectedAt { get; protected set; }
        public override IPEndPoint EndPoint { get; protected set; }
        public override string Error { get; protected set; }

        protected override BaseNetworkServer NetworkServer { get; set; }
        protected override long LastReceiveTime { get; set; }

        public override void Init(BaseNetworkServer networkServer, bool closeMsg)
        {
            NetworkServer = networkServer;
        }

        public override void Update()
        {
            var now = Time.Get();

            if (State == ConnectionState.OFFLINE ||
                State == ConnectionState.ERROR)
            {
                return;
            }

            if (State != ConnectionState.OFFLINE &&
                State != ConnectionState.CONNECT &&
                (now - LastReceiveTime) > Time.Freq() * NetworkServer.Config.ConnectionTimeout)
            {
                State = ConnectionState.ERROR;
                Error = "Timeout";
                return;
            }
        }

        public override void Disconnect(string reason)
        {
            throw new System.NotImplementedException();
        }

        public override bool Feed(NetworkChunkConstruct packet, IPEndPoint remote)
        {
            throw new System.NotImplementedException();
        }

        public override void Flush()
        {
            throw new System.NotImplementedException();
        }

        public override bool QueueChunk(SendFlags flags, byte[] data, int dataSize)
        {
            throw new System.NotImplementedException();
        }
    }
}
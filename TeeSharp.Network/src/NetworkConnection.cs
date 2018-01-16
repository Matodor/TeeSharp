using System.Net;
using System.Net.Sockets;

namespace TeeSharp.Network
{
    public class NetworkConnection : BaseNetworkConnection
    {
        public override ConnectionState State { get; protected set; }
        public override long ConnectedAt { get; protected set; }
        public override IPEndPoint EndPoint { get; protected set; }

        public override void Init(UdpClient client, bool closeMsg)
        {
        }

        public override void Update()
        {
        }
    }
}
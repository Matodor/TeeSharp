using System.Net;
using System.Net.Sockets;
using TeeSharp.Common;

namespace TeeSharp.Network
{
    public enum ConnectionState
    {
        OFFLINE = 0,
        CONNECT,
        PENDING,
        ONLINE,
        ERROR,
    }

    public abstract class BaseNetworkConnection : BaseInterface
    {
        public abstract ConnectionState State { get; protected set; }
        public abstract long ConnectedAt { get; protected set; }
        public abstract IPEndPoint EndPoint { get; protected set; }
        public abstract string Error { get; protected set; }

        public abstract void Init(UdpClient client, bool closeMsg);
        public abstract void Update();
    }
}
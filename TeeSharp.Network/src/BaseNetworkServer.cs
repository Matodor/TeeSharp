using System.Net;
using System.Net.Sockets;
using TeeSharp.Common;

namespace TeeSharp.Network
{
    public delegate void NewClientCallback(int clientId);
    public delegate void DelClientCallback(int clientId, string reason);

    public abstract class BaseNetworkServer : BaseInterface
    {
        public abstract int MaxClientsPerIp { get; protected set; }
        public abstract int MaxClients { get; protected set; }

        protected abstract BaseNetworkConnection[] Connections { get; set; }
        protected abstract UdpClient UdpClient { get; set; }
        protected abstract NewClientCallback NewClientCallback { get; set; }
        protected abstract DelClientCallback DelClientCallback { get; set; }
        
        public abstract void Init();
        public abstract bool Open(IPEndPoint ipEndPoint, int maxClients, int maxClientsPerIp);
        public abstract void SetCallbacks(NewClientCallback newClientCB, DelClientCallback delClientCB);
        public abstract void SetMaxClientsPerIp(int maxClientsPerIp);
        public abstract IPEndPoint ClientAddr(int clientId);
        public abstract AddressFamily NetType();
        public abstract void Update();
        public abstract bool Receive(out NetChunk packet);
        public abstract void Drop(int clientId, string reason);
    }
}

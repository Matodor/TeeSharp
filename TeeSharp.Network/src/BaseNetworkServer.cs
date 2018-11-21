using System.Net;
using System.Net.Sockets;
using TeeSharp.Common.Config;
using TeeSharp.Core;

namespace TeeSharp.Network
{
    public delegate void NewClientCallback(int clientId, bool legacy);
    public delegate void DelClientCallback(int clientId, string reason);

    public struct NetworkServerConfig
    {
        public IPEndPoint LocalEndPoint;
        public int MaxClients;
        public int MaxClientsPerIp;
    }

    public abstract class BaseNetworkServer : BaseInterface
    {
        public abstract NetworkServerConfig ServerConfig { get; protected set; }

        protected abstract BaseChunkReceiver ChunkReceiver { get; set; }
        protected abstract BaseNetworkBan NetworkBan { get; set; }
        protected abstract BaseConfig Config { get; set; }

        protected abstract BaseNetworkConnection[] Connections { get; set; }
        protected abstract UdpClient UdpClient { get; set; }
        protected abstract NewClientCallback NewClientCallback { get; set; }
        protected abstract DelClientCallback DelClientCallback { get; set; }
        
        public abstract void Init();
        public abstract bool Open(NetworkServerConfig config);

        public abstract void SetMaxClientsPerIp(int max);
        public abstract void SetCallbacks(NewClientCallback newClientCB, DelClientCallback delClientCB);

        public abstract IPEndPoint ClientEndPoint(int clientId);
        public abstract AddressFamily NetType();
        public abstract void Update();
        public abstract bool Receive(out NetworkChunk packet);
        public abstract void Drop(int clientId, string reason);
        public abstract int FindSlot(IPEndPoint endPoint, bool comparePorts);
        public abstract void Send(NetworkChunk packet);

        protected abstract NetworkServerConfig CheckConfig(NetworkServerConfig config);
    }
}

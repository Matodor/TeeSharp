using System;
using System.Net;
using System.Net.Sockets;

namespace TeeSharp
{
    public class NetworkServer : INetworkServer
    {
        protected Configuration _config;
        protected UdpClient _udpClient;

        public NetworkServer()
        {
            
        }

        public virtual void Init()
        {
            _config = Kernel.Get<Configuration>();
        }

        public virtual void Open(IPEndPoint endPoint, int maxClients, int maxClientsPerIp)
        {
            if (!Base.CreateUdpClient(endPoint, out _udpClient))
                throw new Exception($"couldn't open socket. port {_config.GetInt("SvPort")} might already be in use");


        }

        public void SetMaxClientsPerIp(int maxClients)
        {
            throw new NotImplementedException();
        }

        public void SetCallbacks(NewClientCallback newClientCallback, DelClientCallback delClientCallback)
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        public void Receive()
        {
            throw new NotImplementedException();
        }

        public void SendPacket(NetChunk chunk)
        {
            throw new NotImplementedException();
        }

        public void Drop(int clientId, string reason)
        {
            throw new NotImplementedException();
        }
    }
}

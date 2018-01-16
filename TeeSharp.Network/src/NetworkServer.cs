using System;
using System.Net;
using System.Net.Sockets;
using TeeSharp.Common;
using Math = System.Math;

namespace TeeSharp.Network
{
    public class NetworkServer : BaseNetworkServer
    {
        public override int MaxClientsPerIp { get; protected set; }
        public override int MaxClients { get; protected set; }

        protected BaseServerBan ServerBan { get; private set; }
        
        protected override UdpClient UdpClient { get; set; }
        protected override NewClientCallback NewClientCallback { get; set; }
        protected override DelClientCallback DelClientCallback { get; set; }
        protected override BaseNetworkConnection[] Connections { get; set; }

        public override void Init()
        {
            ServerBan = Kernel.Get<BaseServerBan>();
        }

        public override bool Open(IPEndPoint localEP, int maxClients, int maxClientsPerIp)
        {
            if (!NetworkHelper.CreateUdpClient(localEP, out var socket))
                return false;

            UdpClient = socket;
            MaxClients = maxClients;
            SetMaxClientsPerIp(maxClientsPerIp);
            Connections = new BaseNetworkConnection[MaxClients];

            for (var i = 0; i < Connections.Length; i++)
            {
                Connections[i] = Kernel.Get<BaseNetworkConnection>();
                Connections[i].Init(UdpClient, true);
            }

            return true;
        }

        public override void SetCallbacks(NewClientCallback newClientCB, DelClientCallback delClientCB)
        {
            NewClientCallback = newClientCB;
            DelClientCallback = delClientCB;
        }

        public override void SetMaxClientsPerIp(int maxClientsPerIp)
        {
            MaxClientsPerIp = Math.Clamp(maxClientsPerIp, 1, MaxClients);
        }

        public override IPEndPoint ClientAddr(int clientId)
        {
            return Connections[clientId].EndPoint;
        }

        public override AddressFamily NetType()
        {
            return UdpClient.Client.AddressFamily;
        }

        public override void Update()
        {
            var now = Time.Get();

            for (var clientId = 0; clientId < Connections.Length; clientId++)
            {
                Connections[clientId].Update();

                if (Connections[clientId].State == ConnectionState.ERROR)
                {
                    if (now - Connections[clientId].ConnectedAt < Time.Freq())
                        ServerBan.BanAddr(ClientAddr(clientId), 60, "Stressing network");
                    else
                        Drop(clientId, Connections[clientId].Error);
                }
            }
        }

        public override bool Receive(out NetChunk packet)
        {
            if (UdpClient.Available == 0)
            {
                packet = null;
                return false;
            }

            return false;
        }

        public override void Drop(int clientId, string reason)
        {
            throw new NotImplementedException();
        }
    }
}
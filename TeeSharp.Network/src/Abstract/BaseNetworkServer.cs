using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TeeSharp.Core;

namespace TeeSharp.Network
{
    public delegate void ClientConnectedEvent(int clientId);
    public delegate void ClientDisconnectedEvent(int clientId, string reason);

    public struct NetworkServerConfig
    {
        public IPEndPoint BindEndPoint;
        public int MaxClients;
        public int MaxClientsPerIp;
        public ConnectionConfig ConnectionConfig;
    }

    public abstract class BaseNetworkServer : BaseInterface
    {
        public virtual event ClientConnectedEvent ClientConnected;
        public virtual event ClientDisconnectedEvent ClientDisconnected;

        public virtual NetworkServerConfig Config { get; protected set; }

        protected virtual UdpClient UdpClient { get; set; }
        protected virtual BaseNetworkBan NetworkBan { get; set; }
        protected virtual IList<BaseNetworkConnection> Connections { get; set; }

        protected virtual BaseChunkReceiver ChunkReceiver { get; set; }

        protected virtual BaseTokenManager TokenManager { get; set; }
        protected virtual BaseTokenCache TokenCache { get; set; }

        public abstract void Init();
        public abstract bool Open(NetworkServerConfig config);

        public abstract void SetMaxClientsPerIp(int max);

        public abstract IPEndPoint ClientEndPoint(int clientId);
        public abstract void Update();
        public abstract bool Receive(ref Chunk packet, ref uint token);
        public abstract void Drop(int clientId, string reason);
        public abstract bool Send(Chunk packet, uint token = 4294967295U);
        public abstract void AddToken(IPEndPoint endPoint, uint token);
        
        protected abstract NetworkServerConfig CheckConfig(
            NetworkServerConfig config);

        protected void OnClientConnected(int clientId)
        {
            ClientConnected?.Invoke(clientId);
        }

        protected void OnClientDisconnected(int clientId, string reason)
        {
            ClientDisconnected?.Invoke(clientId, reason);
        }
    }
}

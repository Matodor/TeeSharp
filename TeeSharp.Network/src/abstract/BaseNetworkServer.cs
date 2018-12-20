using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TeeSharp.Common.Config;
using TeeSharp.Core;

namespace TeeSharp.Network
{
    public delegate void NewClientCallback(int clientId);
    public delegate void DelClientCallback(int clientId, string reason);

    public struct NetworkServerConfig
    {
        public IPEndPoint BindEndPoint;
        public int MaxClients;
        public int MaxClientsPerIp;
    }

    public abstract class BaseNetworkServer : BaseInterface
    {
        public virtual NetworkServerConfig ServerConfig { get; protected set; }

        protected virtual UdpClient UdpClient { get; set; }
        protected virtual BaseNetworkBan NetworkBan { get; set; }
        protected virtual IList<BaseNetworkConnection> Connections { get; set; }

        protected virtual BaseChunkReceiver ChunkReceiver { get; set; }
        protected virtual BaseConfig Config { get; set; }

        protected virtual NewClientCallback ClientConnected { get; set; }
        protected virtual DelClientCallback ClientDisconnected { get; set; }
        
        protected virtual BaseTokenManager TokenManager { get; set; }
        protected virtual BaseTokenCache TokenCache { get; set; }

        public abstract void Init();
        public abstract bool Open(NetworkServerConfig config);

        public abstract void SetMaxClientsPerIp(int max);
        public abstract void SetCallbacks(NewClientCallback newClientCB,
            DelClientCallback delClientCB);

        public abstract IPEndPoint ClientEndPoint(int clientId);
        public abstract void Update();
        public abstract bool Receive(ref Chunk packet, ref uint token);
        public abstract void Drop(int clientId, string reason);
        public abstract bool Send(Chunk packet, uint token = 4294967295U);
        public abstract void AddToken(IPEndPoint endPoint, uint token);
        
        protected abstract NetworkServerConfig CheckConfig(
            NetworkServerConfig config);
    }
}

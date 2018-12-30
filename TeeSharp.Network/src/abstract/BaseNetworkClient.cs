using System.Net;
using System.Net.Sockets;
using TeeSharp.Core;
using TeeSharp.Network.Enums;

namespace TeeSharp.Network
{
    public struct NetworkClientConfig
    {
        public IPEndPoint LocalEndPoint;
        public ConnectionConfig ConnectionConfig;
    }

    public abstract class BaseNetworkClient : BaseInterface
    {
        public virtual ClientState State
        {
            get
            {
                if (Connection.State == ConnectionState.Online)
                    return ClientState.Online;
                if (Connection.State == ConnectionState.Offline)
                    return ClientState.Offline;
                return ClientState.Connecting;
            }
        }

        protected virtual BaseNetworkConnection Connection { get; set; }
        protected virtual BaseChunkReceiver ChunkReceiver { get; set; }
        protected virtual UdpClient UdpClient { get; set; }

        protected virtual BaseTokenCache TokenCache { get; set; }
        protected virtual BaseTokenManager TokenManager { get; set; }
        protected virtual NetworkClientConfig Config { get; set; }

        public abstract void Init();
        public abstract bool Open(NetworkClientConfig config);
        public abstract void Close();

        public abstract void Disconnect(string reason);
        public abstract bool Connect(IPEndPoint endPoint);
        public abstract void Update();

        public abstract bool Receive(ref Chunk packet, ref uint token);
        public abstract void Send(Chunk packet, 
            uint token = TokenHelper.TokenNone,
            SendCallbackData callbackData = null);
        public abstract void PurgeStoredPacket(int trackId);

        public abstract void Flush();
        public abstract bool GotProblems();
    }
}
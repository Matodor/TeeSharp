using System.Net;

namespace TeeSharp
{
    public delegate void NewClientCallback(int clientId);
    public delegate void DelClientCallback(int clientId, string reason);

    public interface INetworkServer
    {
        void Init();
        void Open(IPEndPoint endPoint, int maxClients, int maxClientsPerIp);
        void SetMaxClientsPerIp(int maxClients);
        void SetCallbacks(NewClientCallback newClientCallback, DelClientCallback delClientCallback);

        void Update();
        bool Receive(out NetChunk chunk);
        void Send(NetChunk chunk);

        void Drop(int clientId, string reason);
    }
}

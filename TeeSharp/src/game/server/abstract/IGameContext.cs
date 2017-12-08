using System;

namespace TeeSharp.Server
{
    public interface IGameContext
    {
        IPlayer GetPlayer(int clientId);

        bool IsClientPlayer(int ClientID);
        bool IsClientReady(int ClientID);
        string GameType();

        void OnTick();
        void OnInit();
        void OnShutdown();
        void OnConsoleInit();

        void OnClientEnter(int clientId);
        void OnClientConnected(int clientId);
        void OnClientDrop(int clientId, string reason);
        void OnMessage(NetMessages msg, Unpacker unpacker, int clientId);

        void OnClientPredictedInput(int clientId, int[] data);
        void OnClientDirectInput(int clientId, int[] data);
    }
}

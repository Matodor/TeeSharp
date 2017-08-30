namespace TeeSharp.Server
{
    public interface IGameContext
    {
        IPlayer GetPlayer(int clientId);

        bool IsClientPlayer(int ClientID);
        string GameType();

        void OnTick();
        void OnInit();
        void OnShutdown();
        void OnConsoleInit();

        void OnClientDrop(int clientId, string reason);
        void OnMessage(NetMessages msg, Unpacker unpacker, int clientId);
    }
}

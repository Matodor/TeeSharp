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
    }
}

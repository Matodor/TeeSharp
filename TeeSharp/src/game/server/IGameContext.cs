namespace TeeSharp.Server
{
    public interface IGameContext
    {
        IPlayer GetPlayer(int clientId);
        
        void OnTick();
        void OnInit();
        void OnShutdown();
    }
}

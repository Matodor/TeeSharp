namespace TeeSharp.Server
{
    public interface IGameContext
    {
        Player GetPlayer(int clientId);
    }
}

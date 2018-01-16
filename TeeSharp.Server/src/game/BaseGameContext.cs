using TeeSharp.Common;

namespace TeeSharp.Server.Game
{
    public abstract class BaseGameContext : BaseInterface
    {
        public abstract void RegisterCommands();
        public abstract void OnInit();
        public abstract void OnTick();
        public abstract void OnClientPredictedInput(int clientId, int[] data);
    }
}
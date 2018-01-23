using TeeSharp.Common;
using TeeSharp.Common.Enums;
using TeeSharp.Core;

namespace TeeSharp.Server.Game
{
    public abstract class BaseGameContext : BaseInterface
    {
        public abstract void RegisterCommands();
        public abstract void OnInit();
        public abstract void OnTick();
        public abstract void OnClientPredictedInput(int clientId, int[] data);
        public abstract void OnShutdown();
        public abstract void OnMessage(NetworkMessages message, Unpacker unpacker, int clientId);
        public abstract void OnBeforeSnapshot();
        public abstract void OnAfterSnapshot();
        public abstract void OnSnapshot(int clientId);
    }
}
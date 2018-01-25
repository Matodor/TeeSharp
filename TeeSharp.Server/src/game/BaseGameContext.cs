using TeeSharp.Common;
using TeeSharp.Common.Enums;
using TeeSharp.Common.NetObjects;
using TeeSharp.Core;

namespace TeeSharp.Server.Game
{
    public abstract class BaseGameContext : BaseInterface
    {
        public abstract BaseGameController GameController { get; }
        public abstract string GameVersion { get; }
        public abstract string NetVersion { get; }
        public abstract string ReleaseVersion { get; }
        public abstract BasePlayer[] Players { get; protected set; }

        protected abstract BaseServer Server { get; set; }

        public abstract void RegisterConsoleCommands();
        public abstract bool IsClientInGame(int clientId);
        public abstract bool IsClientReady(int clientId);

        public abstract void OnInit();
        public abstract void OnTick();
        public abstract void OnShutdown();
        public abstract void OnMessage(NetworkMessages message, Unpacker unpacker, int clientId);
        public abstract void OnBeforeSnapshot();
        public abstract void OnAfterSnapshot();
        public abstract void OnSnapshot(int clientId);
        public abstract void OnClientConnected(int clientId);
        public abstract void OnClientEnter(int clientId);
        public abstract void OnClientDrop(int clientId, string reason);
        public abstract void OnClientPredictedInput(int clientId, NetObj_PlayerInput input);
        public abstract void OnClientDirectInput(int clientId, NetObj_PlayerInput input);
    }
}
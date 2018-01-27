using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Console;
using TeeSharp.Common.Protocol;
using TeeSharp.Core;

namespace TeeSharp.Server.Game
{
    public abstract class BaseGameContext : BaseInterface
    {
        public abstract string GameVersion { get; }
        public abstract string NetVersion { get; }
        public abstract string ReleaseVersion { get; }
        public abstract BasePlayer[] Players { get; protected set; }
        public abstract BaseGameController GameController { get; protected set; }

        protected abstract BaseTuningParams Tuning { get; set; }
        protected abstract BaseConfig Config { get; set; }
        protected abstract BaseGameConsole Console { get; set; }
        protected abstract BaseServer Server { get; set; }
        protected abstract BaseLayers Layers { get; set; }
        protected abstract BaseCollision Collision { get; set; }
        protected abstract BaseGameMsgUnpacker GameMsgUnpacker { get; set; }

        public abstract void RegisterConsoleCommands();
        public abstract bool IsClientInGame(int clientId);
        public abstract bool IsClientReady(int clientId);

        public abstract void CheckPureTuning();
        public abstract void SendTuningParams(int clientId);

        public abstract void OnInit();
        public abstract void OnTick();
        public abstract void OnShutdown();
        public abstract void OnMessage(int msgId, Unpacker unpacker, int clientId);
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
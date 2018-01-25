using TeeSharp.Common;
using TeeSharp.Common.Enums;
using TeeSharp.Common.NetObjects;
using TeeSharp.Server.Game.gamemodes;

namespace TeeSharp.Server.Game
{
    public class GameContext : BaseGameContext
    {
        public override BaseGameController GameController { get; }
        public override string GameVersion { get; } = "0.6";
        public override string NetVersion { get; } = "0.6";
        public override string ReleaseVersion { get; } = "0.63";
        public override BasePlayer[] Players { get; protected set; }

        protected override BaseServer Server { get; set; }

        public GameContext()
        {
            GameController = new GameControllerDM();
        }

        public override void RegisterConsoleCommands()
        {
        }

        public override bool IsClientInGame(int clientId)
        {
            return false;
        }

        public override bool IsClientReady(int clientId)
        {
            throw new System.NotImplementedException();
        }

        public override void OnInit()
        {
            Server = Kernel.Get<BaseServer>();
            Players = Server.
        }

        public override void OnTick()
        {
        }

        public override void OnShutdown()
        {
            throw new System.NotImplementedException();
        }

        public override void OnMessage(NetworkMessages message, Unpacker unpacker, int clientId)
        {
            throw new System.NotImplementedException();
        }

        public override void OnBeforeSnapshot()
        {
        }

        public override void OnAfterSnapshot()
        {
        }

        public override void OnSnapshot(int clientId)
        {
            throw new System.NotImplementedException();
        }

        public override void OnClientConnected(int clientId)
        {
            var team = 
        }

        public override void OnClientEnter(int clientId)
        {
            throw new System.NotImplementedException();
        }

        public override void OnClientDrop(int clientId, string reason)
        {
            throw new System.NotImplementedException();
        }

        public override void OnClientPredictedInput(int clientId, NetObj_PlayerInput input)
        {
            throw new System.NotImplementedException();
        }

        public override void OnClientDirectInput(int clientId, NetObj_PlayerInput input)
        {
            throw new System.NotImplementedException();
        }
    }
}
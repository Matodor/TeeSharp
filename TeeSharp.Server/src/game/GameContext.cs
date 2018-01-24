using TeeSharp.Common;
using TeeSharp.Common.Enums;
using TeeSharp.Server.Game.gamemodes;

namespace TeeSharp.Server.Game
{
    public class GameContext : BaseGameContext
    {
        public override BaseGameController GameController { get; }
        public override string GameVersion { get; } = "0.6";
        public override string NetVersion { get; } = "0.6";
        public override string ReleaseVersion { get; } = "0.63";

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

        public override void OnInit()
        {
        }

        public override void OnTick()
        {
        }

        public override void OnClientPredictedInput(int clientId, int[] data)
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
    }
}
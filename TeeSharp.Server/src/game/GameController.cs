using TeeSharp.Common;
using TeeSharp.Common.Enums;

namespace TeeSharp.Server.Game
{
    public class GameController : BaseGameController
    {
        public override string GameType => "Test";

        public override Team StartTeam()
        {
            return Team.Spectators;
        }

        public override bool IsPlayerReadyMode()
        {
            return false;
        }

        public override bool IsGamePaused()
        {
            return false;
        }

        public override bool StartRespawnState()
        {
            return true;
        }

        public override void Tick()
        {
        }

        public override int Score(int clientId)
        {
            return 0;
        }

        public override bool CanSpawn(Team team, int clientId, out Vector2 pos)
        {
            pos = Vector2.zero;
            return true;
        }

        public override void OnReset()
        {
            
        }
    }
}
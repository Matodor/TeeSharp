using System.Runtime.Serialization.Formatters;
using TeeSharp.Common;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;

namespace TeeSharp.Server.Game
{
    public abstract class VanillaController : BaseGameController
    {
        public override void Init()
        {
            base.Init();

            GameOverTick = -1;
            SuddenDeath = 0;
            RoundStartTick = Server.Tick;
            RoundCount = 0;
            GameFlags = GameFlags.NONE;
        }

        public override void Tick()
        {
        }

        public override int GetPlayerScore(int clientId)
        {
            return 5;
        }

        public override bool IsTeamplay()
        {
            return GameFlags.HasFlag(GameFlags.TEAMS);
        }

        public override string GetTeamName(Team team)
        {
            if (IsTeamplay())
            {
                if (team == Team.RED)
                    return "red team";
                return "blue team";
            }

            if (team == Team.SPECTATORS)
                return "spectators";
            return "game";
        }

        public override Team GetAutoTeam(int clientId)
        {
            return Team.SPECTATORS;
        }

        public override bool CheckTeamsBalance()
        {
            return true;
        }

        public override void OnEntity(int entityIndex, Vector2 pos)
        {
            var entity = (MapItems)entityIndex;
        }

        public override void OnPlayerInfoChange(BasePlayer player)
        {

        }

        public override void OnSnapshot(int snappingClient)
        {
            var gameInfo = Server.SnapObject<SnapObj_GameInfo>(0);

            if (gameInfo == null)
                return;

            gameInfo.GameFlags = GameFlags;
            gameInfo.GameStateFlags = 0;

            if (GameOverTick != -1)
                gameInfo.GameStateFlags |= GameStateFlags.GAMEOVER;
            if (SuddenDeath != 0)
                gameInfo.GameStateFlags |= GameStateFlags.SUDDENDEATH;
            if (GameContext.World.IsPaused)
                gameInfo.GameStateFlags |= GameStateFlags.PAUSED;

            gameInfo.RoundStartTick = (int)RoundStartTick;
            gameInfo.WarmupTimer = Warmup;

            gameInfo.ScoreLimit = Config["SvScorelimit"];
            gameInfo.TimeLimit = Config["SvTimelimit"];

            gameInfo.RoundNum = !string.IsNullOrEmpty(Config["SvMaprotation"]) &&
                                Config["SvRoundsPerMap"] != 0
                ? Config["SvRoundsPerMap"]
                : 0;
            gameInfo.RoundCurrent = RoundCount + 1;
        }
    }
}
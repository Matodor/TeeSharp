using System.Collections.Generic;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Enums;
using TeeSharp.Core;
using TeeSharp.Server.Game.Entities;

namespace TeeSharp.Server.Game
{
    public class SpawnEval
    {
        public Vec2 Pos;
        public bool Got;
        public Team FriendlyTeam;
        public float Score;

        public SpawnEval()
        {
            Got = false;
            FriendlyTeam = Team.SPECTATORS;
            Pos = new Vec2(100, 100);
        }
    }

    public abstract class BaseGameController : BaseInterface
    {
        public abstract string GameType { get; }

        protected virtual GameFlags GameFlags { get; set; }
        protected virtual int RoundStartTick { get; set; }
        protected virtual int GameOverTick { get; set; }
        protected virtual int SuddenDeath { get; set; }
        protected virtual int Warmup { get; set; }
        protected virtual int RoundCount { get; set; }

        protected virtual BaseGameContext GameContext { get; set; }
        protected virtual BaseServer Server { get; set; }
        protected virtual BaseConfig Config { get; set; }

        public abstract int GetPlayerScore(int clientId);
        public abstract string GetTeamName(Team team);
        public abstract Team GetAutoTeam(int clientId);
        public abstract bool CheckTeamsBalance();
        public abstract bool CanSpawn(Team team, int clientId, out Vec2 spawnPos);
        public abstract bool CanJoinTeam(int clientId, Team team);
        public abstract bool CanChangeTeam(BasePlayer player, Team joinTeam);
        public abstract bool IsTeamplay();
        public abstract Team ClampTeam(Team team);
        public abstract void Tick();

        public abstract void OnCharacterSpawn(Character character);
        public abstract void OnSnapshot(int snappingClient);
        public abstract void OnEntity(int tileIndex, Vec2 pos);
        public abstract void OnPlayerInfoChange(BasePlayer player);

        protected abstract float EvaluateSpawnPos(SpawnEval eval, Vec2 pos);
        protected abstract void EvaluateSpawnType(SpawnEval eval, IList<Vec2> spawnPos);

        protected BaseGameController()
        {
            Server = Kernel.Get<BaseServer>();
            GameContext = Kernel.Get<BaseGameContext>();
            Config = Kernel.Get<BaseConfig>();
        }
    }
}
using System.Collections.Generic;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;
using TeeSharp.Core;
using TeeSharp.Map.MapItems;
using TeeSharp.Server.Game.Entities;

namespace TeeSharp.Server.Game
{
    public class SpawnEval
    {
        public Vector2 Position { get; set; }
        public bool Got { get; set; }
        public Team FriendlyTeam { get; set; }
        public float Score { get; set; }

        public SpawnEval()
        {
            Got = false;
            FriendlyTeam = Team.Spectators;
            Position = new Vector2(100, 100);
        }
    }

    public class GameInfo
    {
        public int MatchCurrent;
        public int MatchNum;
        public int ScoreLimit;
        public int TimeLimit;
    }

    public abstract class BaseGameController : BaseInterface
    {

        //private string _mapWish;

        //public abstract string GetTeamName(Team team);
        //public abstract Team GetAutoTeam(int clientId);
        //public abstract bool CheckTeamBalance();

        //public abstract bool CanBeMovedOnBalance(int clientId);
        //public abstract bool IsForceBalanced();
        //public abstract bool IsFriendlyFire(int cliendId1, int clientId2);
        //public abstract void PostReset();

        //public abstract Team ClampTeam(Team team);
        //public abstract void Tick();

        //public abstract void OnClientEnter(int clientId);
        //public abstract void OnPlayerReady(int clientId);

        //public abstract void OnCharacterSpawn(Character character);
        //public abstract void OnSnapshot(int snappingClient);
        //public abstract void OnEntity(int tileIndex, Vector2 pos);
        //public abstract void OnPlayerInfoChange(BasePlayer player);
        //public abstract int OnCharacterDeath(Character victim, 
        //    BasePlayer killer, Weapon weapon);

        //protected BaseGameController()
        //{
        //    Server = Kernel.Get<BaseServer>();
        //    GameContext = Kernel.Get<BaseGameContext>();
        //    Config = Kernel.Get<BaseConfig>();
        //    GameWorld = Kernel.Get<BaseGameWorld>();
        //}

        //public virtual void ChangeMap(string toMap)
        //{
        //    _mapWish = toMap;
        //}

        //public virtual void CycleMap()
        //{
        //    // TODO
        //    throw new NotImplementedException();

        //    if (!string.IsNullOrEmpty(_mapWish))
        //    {
        //        GameContext.Console.Print(OutputLevel.Debug, "game", $"rotating map to {_mapWish}");
        //        _mapWish = string.Empty;

        //    }
        //}

        //public virtual bool CanSpawn(Team team, int clientId, out Vector2 spawnPos)
        //{
        //    if (team == Team.Spectators)
        //    {
        //        spawnPos = Vector2.Zero;
        //        return false;
        //    }

        //    var eval = new SpawnEval();

        //    if (IsTeamplay())
        //    {
        //        eval.FriendlyTeam = team;

        //        EvaluateSpawnType(eval, SpawnPos[1 + ((int)team & 1)]);
        //        if (!eval.Got)
        //        {
        //            EvaluateSpawnType(eval, SpawnPos[0]);
        //            if (!eval.Got)
        //                EvaluateSpawnType(eval, SpawnPos[1 + (((int)team + 1) & 1)]);
        //        }
        //    }
        //    else
        //    {
        //        EvaluateSpawnType(eval, SpawnPos[0]);
        //        EvaluateSpawnType(eval, SpawnPos[1]);
        //        EvaluateSpawnType(eval, SpawnPos[2]);
        //    }

        //    spawnPos = eval.Position;
        //    return eval.Got;
        //}

        

        public const int TimerInfinite = -1;
        public const int TimerEnd = 10;

        public abstract string GameType { get; }

        public virtual int MatchCount { get; protected set; }
        public virtual int RoundCount { get; protected set; }

        public virtual int GameStartTick { get; protected set; }
        public virtual bool SuddenDeath { get; protected set; }
        public virtual bool GameRunning { get; protected set; }
        public virtual bool GamePaused { get; protected set; }
        public virtual GameState GameState { get; protected set; }
        public virtual GameFlags GameFlags { get; protected set; }

        protected virtual IList<Vector2>[] SpawnPos { get; set; }

        protected virtual BaseGameContext GameContext { get; set; }
        protected virtual BaseServer Server { get; set; }
        protected virtual BaseGameConsole Console { get; set; }
        protected virtual BaseConfig Config { get; set; }
        protected virtual BaseGameWorld World { get; set; }

        protected virtual int GameStateTimer { get; set; }
        protected virtual GameInfo GameInfo { get; set; }

        protected virtual int[] TeamScore { get; set; }

        public abstract void Init();
        public abstract Team StartTeam();
        public abstract bool GetRespawnDisabled(BasePlayer player);
        public abstract bool IsPlayerReadyMode();
        public abstract bool IsTeamChangeAllowed(BasePlayer player);
        public abstract bool IsTeamplay();
        public abstract bool IsFriendlyFire(int clientId1, int clientId2);
        public abstract int Score(int clientId);

        public abstract void Tick();
        public abstract bool CanJoinTeam(BasePlayer player, Team team);
        public abstract bool CanChangeTeam(BasePlayer player, Team team);
        public abstract bool CanSpawn(Team team, int clientId, out Vector2 spawnPos);
        public abstract bool CanSelfKill(BasePlayer player);

        public abstract void OnReset();

        protected abstract void OnCharacterSpawn(BasePlayer player, Character character);
        protected abstract void OnCharacterDied(Character victim, BasePlayer killer, Weapon weapon, ref int modespecial);
        protected abstract void OnPlayerTeamChanged(BasePlayer player, Team prevteam, Team newteam);
        protected abstract void OnPlayerReady(BasePlayer player);
        protected abstract void OnPlayerEnter(BasePlayer player);

        public abstract void OnPlayerInfoChange(BasePlayer player);
        public abstract void OnPlayerChat(BasePlayer player, GameMsg_ClSay message, out bool isSend);
        public abstract void OnPlayerReadyChange(BasePlayer player);
        public abstract void OnSnapshot(int snappingId, out SnapshotGameData gameData);
        public abstract void OnEntity(Tile tile, Vector2 pos);

        protected abstract void CheckReadyStates();
        protected abstract void UpdateGameInfo(int clientId);
        protected abstract void StartMatch();
        protected abstract void SetGameState(GameState state, int timer);
        protected abstract void SetPlayersReadyState(bool state);
        protected abstract void WincheckMatch();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;
using TeeSharp.Core;
using TeeSharp.Server.Game.Entities;

namespace TeeSharp.Server.Game
{
    //public class SpawnEval
    //{
    //    public Vector2 Pos;
    //    public bool Got;
    //    public Team FriendlyTeam;
    //    public float Score;

    //    public SpawnEval()
    //    {
    //        Got = false;
    //        FriendlyTeam = Team.Spectators;
    //        Pos = new Vector2(100, 100);
    //    }
    //}

    public struct GameInfo
    {
        public int MatchCurrent;
        public int MatchNum;
        public int ScoreLimit;
        public int TimeLimit;
    }

    public abstract class BaseGameController : BaseInterface
    {
        //protected virtual IList<Vector2>[] SpawnPos { get; set; }

        //protected virtual BaseGameWorld GameWorld { get; set; }
        //
        //protected virtual BaseConfig Config { get; set; }

        //private string _mapWish;

        //public abstract int GetScore(int clientId);
        //public abstract string GetTeamName(Team team);
        //public abstract Team GetAutoTeam(int clientId);
        //public abstract bool CheckTeamBalance();

        //public abstract bool CanJoinTeam(int clientId, Team team);
        //public abstract bool CanChangeTeam(BasePlayer player, Team joinTeam);

        //public abstract bool CanBeMovedOnBalance(int clientId);
        //public abstract bool IsTeamplay();
        //public abstract bool IsForceBalanced();
        //public abstract bool IsFriendlyFire(int cliendId1, int clientId2);
        //public abstract void PostReset();

        //public abstract Team ClampTeam(Team team);
        //public abstract void Tick();

        //public abstract void OnClientEnter(int clientId);
        //public abstract void OnClientConnected(int clientId);

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
        //        spawnPos = Vector2.zero;
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

        //    spawnPos = eval.Pos;
        //    return eval.Got;
        //}

        //protected virtual float EvaluateSpawnPos(SpawnEval eval, Vector2 pos)
        //{
        //    var score = 0f;

        //    foreach (var character in GameContext.World.GetEntities<Character>())
        //    {
        //        var scoremod = 1f;
        //        if (eval.FriendlyTeam != Team.Spectators && character.Player.Team == eval.FriendlyTeam)
        //            scoremod = 0.5f;

        //        var d = MathHelper.Distance(pos, character.Position);
        //        score += scoremod * (System.Math.Abs(d) < 0.00001 ? 1000000000.0f : 1.0f / d);
        //    }

        //    return score;
        //}

        //protected virtual void EvaluateSpawnType(SpawnEval eval, IList<Vector2> spawnPos)
        //{
        //    for (var i = 0; i < spawnPos.Count; i++)
        //    {
        //        var positions = new[]
        //        {
        //            new Vector2(0.0f, 0.0f),
        //            new Vector2(-32.0f, 0.0f),
        //            new Vector2(0.0f, -32.0f),
        //            new Vector2(32.0f, 0.0f),
        //            new Vector2(0.0f, 32.0f)
        //        };  // start, left, up, right, down

        //        var result = -1;
        //        var characters = GameContext.World
        //            .FindEntities<Character>(spawnPos[i], 64f)
        //            .ToArray();

        //        for (var index = 0; index < 5 && result == -1; ++index)
        //        {
        //            result = index;

        //            for (var c = 0; c < characters.Length; c++)
        //            {
        //                if (GameContext.MapCollision.IsTileSolid(spawnPos[i] + positions[index]) ||
        //                    MathHelper.Distance(characters[c].Position, spawnPos[i] + positions[index]) <=
        //                    characters[c].ProximityRadius)
        //                {
        //                    result = -1;
        //                    break;
        //                }
        //            }

        //            if (result == -1)
        //                continue;

        //            var p = spawnPos[i] + positions[index];
        //            var s = EvaluateSpawnPos(eval, p);

        //            if (!eval.Got || eval.Score > s)
        //            {
        //                eval.Got = true;
        //                eval.Score = s;
        //                eval.Pos = p;
        //            }
        //        }
        //    }
        //}

        public abstract string GameType { get; }

        public virtual bool GameRunning { get; protected set; }
        public virtual bool GamePaused { get; protected set; }
        public virtual GameState GameState { get; protected set; }
        public virtual GameFlags GameFlags { get; protected set; }

        protected virtual BaseGameContext GameContext { get; set; }
        protected virtual BaseServer Server { get; set; }
        protected virtual BaseGameConsole Console { get; set; }

        protected virtual GameInfo GameInfo { get; set; }

        public abstract void Init();
        public abstract Team StartTeam();
        public abstract bool IsPlayerReadyMode();
        public abstract bool StartRespawnState();

        public abstract void Tick();
        public abstract int Score(int clientId);
        public abstract bool CanSpawn(Team team, int clientId, out Vector2 pos);
        public abstract void OnReset();
        public abstract void OnPlayerInfoChange(BasePlayer player);
        public abstract void OnPlayerConnected(BasePlayer player);
        public abstract void OnPlayerEnter(BasePlayer player);
        public abstract void OnPlayerDisconnected(BasePlayer player);
        public abstract void OnSnapshot(int snappingId, out SnapshotGameData gameData);

        protected abstract void UpdateGameInfo(int clientId);
    }
}
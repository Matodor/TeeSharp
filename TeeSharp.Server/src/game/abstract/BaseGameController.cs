// ReSharper disable UnusedMemberInSuper.Global
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
            Score = 0;
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
        public const int TimerInfinite = -1;
        public const int TimerEnd = 10;
        public const int BalanceCheck = -2;
        public const int BalanceOk = -1;

        public abstract string GameType { get; }
        public virtual bool GamePaused { get; }
        public virtual bool GameRunning { get; }

        public virtual int MatchCount { get; protected set; }
        public virtual int RoundCount { get; protected set; }

        public virtual int GameStartTick { get; protected set; }
        public virtual bool SuddenDeath { get; protected set; }
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
        protected virtual int[] TeamSize { get; set; }
        protected virtual int[] TeamScore { get; set; }
        protected virtual int[] Scores { get; set; }
        protected virtual int[] ScoresStartTick { get; set; }
        protected virtual int UnbalancedTick { get; set; }

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

        protected abstract void OnCharacterSpawn(BasePlayer player, Character character);
        protected abstract void OnCharacterDied(Character victim, BasePlayer killer, Weapon weapon, ref int modespecial);
        protected abstract void OnPlayerTeamChanged(BasePlayer player, Team prevTeam, Team newTeam);
        protected abstract void OnPlayerReady(BasePlayer player);
        protected abstract void OnPlayerEnter(BasePlayer player);
        protected abstract void OnPlayerLeave(BasePlayer player, string reason);

        public abstract void OnPlayerInfoChange(BasePlayer player);
        public abstract void OnPlayerChat(BasePlayer player, GameMsg_ClSay message, out bool isSend);
        public abstract void OnPlayerReadyChange(BasePlayer player);
        public abstract void OnSnapshot(int snappingId, out SnapshotGameData gameData);
        public abstract void OnEntity(Tile tile, Vector2 pos);

        protected abstract void CheckReadyStates(int withoutId = -1);
        protected abstract void UpdateGameInfo(int clientId);
        protected abstract void StartMatch();
        protected abstract void SetGameState(GameState state, int timer);
        protected abstract void SetPlayersReadyState(bool state);
        protected abstract void WincheckMatch();
        protected abstract void StartRound();
        protected abstract void CycleMap();
        protected abstract void SwapTeams();
        protected abstract bool HasEnoughPlayers();
        protected abstract void CheckTeamBalance();
        protected abstract void DoTeamBalance();
        protected abstract void WorldOnReseted();
        protected abstract void WincheckRound();
        protected abstract void DoActivityCheck();
        protected abstract void CheckGameInfo();
        protected abstract void ResetGame();
        protected abstract bool GetPlayersReadyState(int withoutID = -1);
        protected abstract bool CanBeMovedOnBalance(int clientId);
        protected abstract void EndMatch();
        protected abstract void EndRound();
    }
}
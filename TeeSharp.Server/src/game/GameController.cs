using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;

namespace TeeSharp.Server.Game
{
    // TODO abstract
    public class GameController : BaseGameController
    {
        public override string GameType => "Test";

        public override void Init()
        {
            GameContext = Kernel.Get<BaseGameContext>();
            Server = Kernel.Get<BaseServer>();
            Console = Kernel.Get<BaseGameConsole>();
            Config = Kernel.Get<BaseConfig>();

            GameState = GameState.GameRunning;
            GameStateTimer = TimerInfinite;
            GameStartTick = Server.Tick;

            MatchCount = 0;
            RoundCount = 0;
            SuddenDeath = false;

            TeamScore = new int[2];

            if (Config["SvWarmup"])
                SetGameState(GameState.WarmupUser, Config["SvWarmup"]);
            else
                SetGameState(GameState.WarmupGame, TimerInfinite);

            GameFlags = GameFlags.None;
            GameInfo = new GameInfo
            {
                MatchCurrent = MatchCount + 1,
                MatchNum = !string.IsNullOrEmpty(Config["SvMaprotation"]) && Config["SvMatchesPerMap"] != 0 
                    ? Config["SvMatchesPerMap"] 
                    : 0,
                ScoreLimit = Config["SvScorelimit"],
                TimeLimit = Config["SvTimelimit"],
            };
        }

        protected override void SetGameState(GameState state, int timer)
        {

        }

        public override Team StartTeam()
        {
            return Team.Spectators;
        }

        public override bool IsTeamChangeAllowed(BasePlayer player)
        {
            return true;
        }

        public override void TeamChange(BasePlayer player, Team team)
        {
            if (player.Team == team)
                return;

            var prevTeam = player.Team;
            player.SetTeam(team);

            Console.Print(OutputLevel.Debug, "game", $"team join player={player.ClientId}:{Server.ClientName(player.ClientId)} team={team}");

            if (team != Team.Spectators)
            {
                player.IsReadyToPlay = !IsPlayerReadyMode();
                if (GameFlags.HasFlag(GameFlags.Survival))
                    player.RespawnDisabled = GetRespawnDisabled(player);
            }

            OnPlayerTeamChange(player, prevTeam, team);
        }

        protected override void OnPlayerTeamChange(BasePlayer player, Team prevTeam, Team team)
        {
        }

        public override bool CanChangeTeam(BasePlayer player, Team team)
        {
            return true;
        }

        public override bool CanJoinTeam(BasePlayer player, Team team)
        {
            return true;
        }

        public override bool IsPlayerReadyMode()
        {
            return false;
        }

        public override bool GetRespawnDisabled(BasePlayer player)
        {
            if (GameFlags.HasFlag(GameFlags.Survival))
            {
                if (GameState == GameState.WarmupGame ||
                    GameState == GameState.WarmupUser ||
                    GameState == GameState.StartCountdown && GameStartTick == Server.Tick)
                {
                    return false;
                }

                return true;
            }

            return false;
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

        public override void OnPlayerChat(BasePlayer player, GameMsg_ClSay message, out bool isSend)
        {
            isSend = true;
        }

        public override void OnPlayerInfoChange(BasePlayer player)
        {
            
        }

        public override void OnPlayerConnected(BasePlayer player)
        {
            
        }

        public override void OnPlayerEnter(BasePlayer player)
        {
            player.Respawn();
            Console.Print(OutputLevel.Debug, "game", 
                $"team_join player='{player.ClientId}:{Server.ClientName(player.ClientId)}' team={player.Team}");
            UpdateGameInfo(player.ClientId);
        }

        protected override void UpdateGameInfo(int clientId)
        {
            var msg = new GameMsg_SvGameInfo()
            {
                GameFlags = GameFlags,
                ScoreLimit = GameInfo.ScoreLimit,
                TimeLimit = GameInfo.TimeLimit,
                MatchCurrent = GameInfo.MatchCurrent,
                MatchNum = GameInfo.MatchNum,
            };

            if (clientId == -1)
            {
                for (var i = 0; i < GameContext.Players.Length; i++)
                {
                    if (GameContext.Players[i] == null || !Server.ClientInGame(i))
                        continue;

                    Server.SendPackMsg(msg, MsgFlags.Vital | MsgFlags.NoRecord, i);
                }
            }
            else
            {
                Server.SendPackMsg(msg, MsgFlags.Vital | MsgFlags.NoRecord, clientId);
            }
        }

        public override void OnPlayerDisconnected(BasePlayer player)
        {
            
        }

        public override void OnSnapshot(int snappingId, out SnapshotGameData gameData)
        {
            gameData = Server.SnapshotItem<SnapshotGameData>(0);
            if (gameData == null)
                return;
            
            gameData.GameStartTick = 0; // GameStartTick
            gameData.GameStateFlags = GameStateFlags.None;
            gameData.GameStateEndTick = 0;
        }
    }
}
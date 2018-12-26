using TeeSharp.Common;
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
        }

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
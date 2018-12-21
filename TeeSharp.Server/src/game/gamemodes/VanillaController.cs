using System.Collections.Generic;
using TeeSharp.Common;
using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Server.Game.Entities;
using Math = System.Math;

namespace TeeSharp.Server.Game
{
    public abstract class VanillaController : BaseGameController
    {
        protected virtual GameFlags GameFlags { get; set; }
        protected virtual int RoundStartTick { get; set; }
        protected virtual int GameOverTick { get; set; }
        protected virtual int SuddenDeath { get; set; }
        protected virtual int Warmup { get; set; }
        protected virtual int RoundCount { get; set; }
        protected virtual int UnbalancedTick { get; set; }
        
        protected virtual bool ForceBalanced { get; set; }
        protected virtual int[] TeamScores { get; set; }

        protected virtual int[] Scores { get; set; }
        protected virtual float[] ScoresStartTick { get; set; }

        protected VanillaController()
        {
            SpawnPos = new IList<Vector2>[]
            {
                new List<Vector2>(), // default spawn pos
                new List<Vector2>(), // red team spawn pos
                new List<Vector2>()  // blue team spawn pos
            };

            Scores = new int[GameContext.Players.Length];
            ScoresStartTick = new float[GameContext.Players.Length];

            DoWarmup(Config["SvWarmup"]);

            TeamScores = new int[2];
            TeamScores[(int) Team.Red] = 0;
            TeamScores[(int) Team.Blue] = 0;

            GameOverTick = -1;
            SuddenDeath = 0;
            RoundStartTick = Server.Tick;
            RoundCount = 0;
            GameFlags = GameFlags.NONE;

            UnbalancedTick = -1;
            ForceBalanced = false;
        }

        public override void OnClientEnter(int clientId)
        {
        }

        public override void OnClientConnected(int clientId)
        {
            Scores[clientId] = 0;
            ScoresStartTick[clientId] = Server.Tick;
        }

        protected virtual void DoWarmup(int seconds)
        {
            if (seconds < 0)
                Warmup = 0;
            else
                Warmup = seconds * Server.TickSpeed;
        }

        protected virtual void DoTeamBalance()
        {
            if (IsTeamplay() && UnbalancedTick != -1 &&
                Server.Tick > UnbalancedTick + Config["SvTeambalanceTime"] * Server.TickSpeed * 60)
            {
                GameContext.Console.Print(OutputLevel.DEBUG, "game", "Balancing teams");

                var teamPlayers = new int[] { 0, 0 };
                var teamScores = new float[] { 0, 0 };
                var playersScores = new float[GameContext.Players.Length];

                for (var i = 0; i < GameContext.Players.Length; i++)
                {
                    if (GameContext.Players[i] == null ||
                        GameContext.Players[i].Team == Team.Spectators)
                    {
                        continue;
                    }

                    teamPlayers[(int)GameContext.Players[i].Team]++;
                    playersScores[i] = GetPlayerScore(i) * Server.TickSpeed * 60f /
                                       (Server.Tick - ScoresStartTick[i]);
                    teamScores[(int)GameContext.Players[i].Team] += playersScores[i];
                }

                if (Math.Abs(teamPlayers[0] - teamPlayers[1]) >= 2)
                {
                    var m = teamPlayers[0] > teamPlayers[1] ? Team.Red : Team.Blue;
                    var numBalance = Math.Abs(teamPlayers[0] - teamPlayers[1]) / 2;

                    do
                    {
                        BasePlayer p = null;
                        var pd = teamScores[(int)m];

                        for (var i = 0; i < GameContext.Players.Length; i++)
                        {
                            if (GameContext.Players[i] == null || !CanBeMovedOnBalance(i))
                                continue;

                            if (GameContext.Players[i].Team == m &&
                                (p == null || Math.Abs(
                                     (teamScores[(int)m ^ 1] + playersScores[i]) -
                                     (teamScores[(int)m] - playersScores[i])
                                 ) < pd))
                            {
                                p = GameContext.Players[i];
                                pd = Math.Abs((teamScores[(int)m ^ 1] + playersScores[i]) -
                                              (teamScores[(int)m] - playersScores[i]));
                            }
                        }

                        var tmp = p.LastActionTick;
                        p.SetTeam((Team)((int)m ^ 1));
                        p.LastActionTick = tmp;
                        p.Respawn();
                    } while (--numBalance != 0);

                    ForceBalanced = true;
                }

                UnbalancedTick = -1;
            }
        }

        protected virtual void CheckInactivePlayers()
        {
            if (Config["SvInactiveKickTime"] <= 0)
                return;

            for (var i = 0; i < GameContext.Players.Length; i++)
            {
                
            }
        }

        public override void Tick()
        {
            if (Warmup > 0)
            {
                Warmup--;
                if (Warmup == 0)
                    StartRound();
            }

            if (GameOverTick != -1)
            {
                if (Server.Tick < GameOverTick + Server.TickSpeed * 10)
                {
                    CycleMap();
                    StartRound();
                    RoundCount++;
                }
            }

            if (GameWorld.IsPaused)
            {
                RoundStartTick++;
                for (var i = 0; i < ScoresStartTick.Length; i++)
                    ScoresStartTick[i]++;
            }

            DoTeamBalance();
            CheckInactivePlayers();
            DoWincheck();
        }

        protected virtual void CycleMap()
        {
            
        }

        protected virtual void DoWincheck()
        {
            if (GameOverTick != -1 || Warmup != 0 || GameWorld.ResetRequested)
                return;

            if (IsTeamplay())
            {
                if (Config["SvScorelimit"] > 0 &&
                    (TeamScores[(int) Team.Red] >= Config["SvScorelimit"] ||
                     TeamScores[(int) Team.Blue] >= Config["SvScorelimit"]) ||
                    Config["SvTimelimit"] > 0 && Server.Tick - RoundStartTick >=
                    Config["SvTimelimit"] * Server.TickSpeed * 60)
                {
                    if (TeamScores[(int) Team.Red] != TeamScores[(int) Team.Blue])
                        EndRound();
                    else
                        SuddenDeath = 1;
                }
            }
            else
            {
                var topScore = 0;
                var topScoreCount = 0;

                for (var i = 0; i < GameContext.Players.Length; i++)
                {
                    if (GameContext.Players[i] == null)
                        continue;

                    if (GetPlayerScore(i) > topScore)
                    {
                        topScore = GetPlayerScore(i);
                        topScoreCount = 1;
                    }
                    else if (GetPlayerScore(i) == topScore)
                        topScore++;
                }

                if (Config["SvScorelimit"] > 0 && topScore >= Config["SvScorelimit"] ||
                    Config["SvTimelimit"] > 0 && Server.Tick - RoundStartTick >=
                    Config["SvTimelimit"] * Server.TickSpeed * 60)
                {
                    if (topScoreCount == 1)
                        EndRound();
                    else
                        SuddenDeath = 1;
                }
            }
        }

        protected virtual void EndRound()
        {
            if (Warmup > 0)
                return;

            GameWorld.IsPaused = true;
            GameOverTick = Server.Tick;
            SuddenDeath = 0;
        }

        protected virtual void ResetGame()
        {
            GameWorld.ResetRequested = true;
        }

        protected virtual void StartRound()
        {
            ResetGame();

            RoundStartTick = Server.Tick;
            SuddenDeath = 0;
            GameOverTick = -1;
            GameWorld.IsPaused = false;
            TeamScores[(int) Team.Red] = 0;
            TeamScores[(int) Team.Blue] = 0;
            ForceBalanced = false;
            GameContext.Console.Print(OutputLevel.DEBUG, "game", $"start round type='{GameType}' teamplay='{GameFlags.HasFlag(GameFlags.TEAMS)}'");
        }

        public override int GetPlayerScore(int clientId)
        {
            return Scores[clientId];
        }

        public override void PostReset()
        {
            for (var i = 0; i < GameContext.Players.Length; i++)
            {
                if (GameContext.Players[i] == null)
                    continue;

                Scores[i] = 0;
                ScoresStartTick[i] = Server.Tick;

                GameContext.Players[i].Respawn();
                GameContext.Players[i].RespawnTick = Server.Tick + Server.TickSpeed / 2;
            }
        }
        
        public override bool CanChangeTeam(BasePlayer player, Team joinTeam)
        {
            if (!IsTeamplay() || joinTeam == Team.Spectators || Config["SvTeambalanceTime"])
                return true;

            var teamPlayers = new int[] {0, 0};

            for (var i = 0; i < GameContext.Players.Length; i++)
            {
                if (GameContext.Players[i] == null)
                    continue;

                if (GameContext.Players[i].Team != Team.Spectators)
                    teamPlayers[(int) GameContext.Players[i].Team]++;
            }

            teamPlayers[(int) joinTeam]++;

            if (player.Team != Team.Spectators)
                teamPlayers[(int) joinTeam ^ 1]--;

            if (System.Math.Abs(teamPlayers[0] - teamPlayers[1]) >= 2)
            {
                return teamPlayers[0] < teamPlayers[1] && joinTeam == Team.Red ||
                       teamPlayers[0] > teamPlayers[1] && joinTeam == Team.Blue;
            }
            return true;
        }

        public override Team GetAutoTeam(int clientId)
        {
            var numPlayers = new[] { 0, 0 };
            for (var i = 0; i < GameContext.Players.Length; i++)
            {
                if (GameContext.Players[i] == null || clientId == i)
                    continue;

                if (GameContext.Players[i].Team >= Team.Red &&
                    GameContext.Players[i].Team <= Team.Blue)
                {
                    numPlayers[(int)GameContext.Players[i].Team]++;
                }
            }

            var team = (Team) 0;
            if (IsTeamplay())
            {
                team = numPlayers[(int) Team.Red] > numPlayers[(int) Team.Blue]
                    ? Team.Blue
                    : Team.Red;
            }

            return CanJoinTeam(clientId, team) 
                ? team 
                : Team.Spectators;
        }

        public override bool CanJoinTeam(int clientId, Team team)
        {
            if (team == Team.Spectators ||
                GameContext.Players[clientId] != null &&
                GameContext.Players[clientId].Team != Team.Spectators)
            {
                return true;
            }

            var numPlayers = new[] {0, 0};
            for (var i = 0; i < GameContext.Players.Length; i++)
            {
                if (GameContext.Players[i] == null || clientId == i) 
                    continue;

                if (GameContext.Players[i].Team >= Team.Red &&
                    GameContext.Players[i].Team <= Team.Blue)
                {
                    numPlayers[(int) GameContext.Players[i].Team]++;
                }
            }

            return numPlayers[0] + numPlayers[1] < Server.MaxClients - Config["SvSpectatorSlots"];
        }

        public override bool CanBeMovedOnBalance(int clientId)
        {
            return true;
        }

        public override bool IsForceBalanced()
        {
            if (!ForceBalanced)
                return false;

            ForceBalanced = false;
            return true;
        }

        public override bool IsFriendlyFire(int cliendId1, int clientId2)
        {
            if (cliendId1 == clientId2)
                return false;

            if (IsTeamplay())
            {
                if (GameContext.Players[cliendId1] == null ||
                    GameContext.Players[clientId2] == null)
                {
                    return false;
                }

                if (GameContext.Players[cliendId1].Team == GameContext.Players[clientId2].Team)
                    return true;
            }

            return false;
        }

        public override bool IsTeamplay()
        {
            return GameFlags.HasFlag(GameFlags.TEAMS);
        }

        public override Team ClampTeam(Team team)
        {
            if (team < 0)
                return Team.Spectators;
            if (IsTeamplay())
                return (Team) ((int) team & 1);
            return 0;
        }

        public override string GetTeamName(Team team)
        {
            if (IsTeamplay())
                return team == Team.Red ? "red team" : "blue team";
            return team == Team.Spectators ? "spectators" : "game";
        }

        public override bool CheckTeamBalance()
        {
            if (!IsTeamplay() || Config["SvTeambalanceTime"] == 0)
                return true;

            var teamPlayers = new int[] {0, 0};
            for (var i = 0; i < GameContext.Players.Length; i++)
            {
                if (GameContext.Players[i] == null ||
                    GameContext.Players[i].Team == Team.Spectators)
                {
                    continue;
                }

                teamPlayers[(int) GameContext.Players[i].Team]++;
            }

            if (Math.Abs(teamPlayers[0] - teamPlayers[1]) >= 2)
            {
                GameContext.Console.Print(OutputLevel.DEBUG, "game", $"Teams are not balanced (red={teamPlayers[0]} blue={teamPlayers[1]})");
                if (UnbalancedTick == -1)
                    UnbalancedTick = Server.Tick;
                return false;
            }

            GameContext.Console.Print(OutputLevel.DEBUG, "game", $"Team are balanced (red={teamPlayers[0]} blue={teamPlayers[1]})");
            UnbalancedTick = -1;
            return true;
        }

        public override void OnCharacterSpawn(Character character)
        {
            character.IncreaseHealth(10);
            character.GiveWeapon(Weapon.Hammer, -1);
            character.GiveWeapon(Weapon.Gun, 10);
        }

        public override int OnCharacterDeath(Character victim, BasePlayer killer, Weapon weapon)
        {
            if (killer == null || weapon == Weapon.Game)
                return 0;

            if (killer == victim.Player)
            {
                Scores[victim.Player.ClientId]--;
            }
            else
            {
                if (IsTeamplay() && victim.Player.Team == killer.Team)
                {
                    Scores[killer.ClientId]--;
                }
                else
                {
                    Scores[killer.ClientId]++;
                }
            }

            if (weapon == Weapon.Self)
                victim.Player.RespawnTick = Server.Tick + Server.TickSpeed * 3;

            return 0;
        }

        public override void OnEntity(int entityIndex, Vector2 pos)
        {
            var item = (MapItems) entityIndex;
            var powerup = Powerup.None;
            var weapon = Weapon.Hammer;

            switch (item)
            {
                case MapItems.ENTITY_SPAWN:
                    SpawnPos[0].Add(pos);
                    break;

                case MapItems.ENTITY_SPAWN_RED:
                    SpawnPos[1].Add(pos);
                    break;

                case MapItems.ENTITY_SPAWN_BLUE:
                    SpawnPos[2].Add(pos);
                    break;

                case MapItems.ENTITY_ARMOR_1:
                    powerup = Powerup.Armor;
                    break;

                case MapItems.ENTITY_HEALTH_1:
                    powerup = Powerup.Health;
                    break;

                case MapItems.ENTITY_WEAPON_SHOTGUN:
                    powerup = Powerup.Weapon;
                    weapon = Weapon.Shotgun;
                    break;

                case MapItems.ENTITY_WEAPON_GRENADE:
                    powerup = Powerup.Weapon;
                    weapon = Weapon.Grenade;
                    break;

                case MapItems.ENTITY_POWERUP_NINJA:
                    powerup = Powerup.Ninja;
                    weapon = Weapon.Ninja;
                    break;

                case MapItems.ENTITY_WEAPON_RIFLE:
                    powerup = Powerup.Weapon;
                    weapon = Weapon.Laser;
                    break;
            }

            if (powerup != Powerup.None)
            {
                new Pickup(powerup, weapon)
                {
                    Position = pos
                };
            }
        }

        public override void OnPlayerInfoChange(BasePlayer player)
        {
            var teamColors = new int[] {65387, 10223467};
            if (!IsTeamplay())
                return;

            player.TeeInfo.UseCustomColor = true;

            if (player.Team >= Team.Red && player.Team <= Team.Blue)
            {
                player.TeeInfo.ColorBody = teamColors[(int) player.Team];
                player.TeeInfo.ColorFeet = teamColors[(int) player.Team];
            }
            else
            {
                player.TeeInfo.ColorBody = 12895054;
                player.TeeInfo.ColorFeet = 12895054;
            }
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
                gameInfo.GameStateFlags |= GameStateFlags.SuddenDeath;
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
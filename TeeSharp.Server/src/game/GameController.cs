using System;
using System.Collections.Generic;
using TeeSharp.Common;
using TeeSharp.Common.Config;
using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;
using TeeSharp.Map.MapItems;
using TeeSharp.Server.Game.Entities;
using Pickup = TeeSharp.Common.Enums.Pickup;

namespace TeeSharp.Server.Game
{
    // TODO abstract
    public class GameController : BaseGameController
    {
        public override string GameType => "Test";
        public override bool GamePaused => GameState == GameState.GamePaused || GameState == GameState.StartCountdown;
        public override bool GameRunning => GameState == GameState.GameRunning;

        public override void Init()
        {
            GameContext = Kernel.Get<BaseGameContext>();
            Server = Kernel.Get<BaseServer>();
            Console = Kernel.Get<BaseGameConsole>();
            Config = Kernel.Get<BaseConfig>();
            World = Kernel.Get<BaseGameWorld>();

            GameState = GameState.GameRunning;
            GameStateTimer = TimerInfinite;
            GameStartTick = Server.Tick;
            UnbalancedTick = BalanceOk;

            MatchCount = 0;
            RoundCount = 0;
            SuddenDeath = false;

            TeamScore = new int[2];
            TeamSize = new int[2];
            Scores = new int[GameContext.Players.Length];
            ScoresStartTick = new int[GameContext.Players.Length];
            SpawnPos = new IList<Vector2>[3]
            {
                new List<Vector2>(), // dm
                new List<Vector2>(), // red
                new List<Vector2>(), // blue
            };

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

            World.Reseted += WorldOnReseted;
            GameContext.PlayerReady += OnPlayerReady;
            GameContext.PlayerEnter += OnPlayerEnter;
            GameContext.PlayerLeave += OnPlayerLeave;
        }

        protected override void ResetGame()
        {
            World.ResetRequested = true;
            SetGameState(GameState.GameRunning, 0);
            GameStartTick = Server.Tick;
            SuddenDeath = false;

            DoTeamBalance();
        }
        
        protected override void WorldOnReseted()
        {
            for (var i = 0; i < GameContext.Players.Length; i++)
            {
                if (GameContext.Players[i] == null)
                    continue;

                GameContext.Players[i].RespawnDisabled = false;
                GameContext.Players[i].Respawn();
                GameContext.Players[i].RespawnTick = Server.Tick + Server.TickSpeed / 2;
                GameContext.Players[i].IsReadyToPlay = true;

                if (RoundCount == 0)
                {
                    Scores[i] = 0;
                    ScoresStartTick[i] = Server.Tick;
                }
            }
        }

        protected override void OnPlayerLeave(BasePlayer player, string reason)
        {
            if (Server.ClientInGame(player.ClientId))
            {
                Console.Print(OutputLevel.Standard, "game", $"leave player={player.ClientId}:{Server.ClientName(player.ClientId)}");
            }

            if (player.Team != Team.Spectators)
            {
                TeamSize[(int) player.Team]--;
                UnbalancedTick = BalanceCheck;
            }

            CheckReadyStates(player.ClientId);

            player.CharacterSpawned -= OnCharacterSpawn;
            player.TeamChanged -= OnPlayerTeamChanged;
        }

        protected override void SetGameState(GameState state, int timer)
        {
            void CheckGameFlagsSurvival()
            {
                if (GameFlags.HasFlag(GameFlags.Survival))
                {
                    for (var i = 0; i < GameContext.Players.Length; i++)
                    {
                        if (GameContext.Players[i] != null)
                            GameContext.Players[i].RespawnDisabled = false;
                    }
                }
            }

            switch (state)
            {
                case GameState.WarmupGame:
                {
                    if (GameState != GameState.GameRunning && 
                        GameState != GameState.WarmupGame &&
                        GameState != GameState.WarmupUser)
                    {
                        return;
                    }

                    if (timer == TimerInfinite)
                    {
                        GameState = state;
                        GameStateTimer = timer;
                        CheckGameFlagsSurvival();
                    }
                    else if (timer == 0)
                    {
                        StartMatch();
                    }
                } break;

                case GameState.WarmupUser:
                {
                    if (GameState != GameState.GameRunning &&
                        GameState != GameState.WarmupUser)
                    {
                        return;
                    }

                    if (timer == 0)
                    {
                        StartMatch();
                        return;
                    }

                    if (timer < 0)
                    {
                        GameStateTimer = TimerInfinite;

                        if (Config["SvPlayerReadyMode"])
                            SetPlayersReadyState(false);
                    }
                    else
                    {
                        GameStateTimer = timer * Server.TickSpeed;
                    }

                    GameState = state;
                    CheckGameFlagsSurvival();
                } break;

                case GameState.StartCountdown:
                {
                    if (GameState != GameState.GameRunning &&
                        GameState != GameState.GamePaused &&
                        GameState != GameState.StartCountdown)
                    {
                        return;
                    }

                    GameState = state;
                    GameStateTimer = Server.TickSpeed * 3;
                    World.Paused = true;
                } break;

                case GameState.GameRunning:
                {
                    GameState = state;
                    GameStateTimer = TimerInfinite;
                    World.Paused = false;
                    SetPlayersReadyState(true);
                } break;

                case GameState.GamePaused:
                {
                    if (GameState != GameState.GameRunning &&
                        GameState != GameState.GamePaused)
                    {
                        return;
                    }

                    if (timer == 0)
                    {
                        SetGameState(GameState.StartCountdown, 0);
                        return;
                    }

                    if (timer < 0)
                    {
                        GameStateTimer = TimerInfinite;
                        SetPlayersReadyState(false);
                    }
                    else
                    {
                        GameStateTimer = timer * Server.TickSpeed;
                    }

                    GameState = state;
                    World.Paused = true;
                } break;

                case GameState.EndRound:
                case GameState.EndMatch:
                {
                    WincheckMatch();

                    if (GameState == GameState.EndMatch)
                        break;

                    if (GameState != GameState.GameRunning &&
                        GameState != GameState.EndRound ||
                        GameState != GameState.EndMatch ||
                        GameState != GameState.GamePaused)
                    {
                        return;
                    }

                    GameState = state;
                    GameStateTimer = timer * Server.TickSpeed;
                    SuddenDeath = false;
                    World.Paused = true;
                } break;
            }
        }

        protected override void WincheckRound()
        {

        }

        protected override void EndMatch()
        {

        }

        protected override void WincheckMatch()
        {
            if (IsTeamplay())
            {
                if (GameInfo.ScoreLimit > 0 && (
                        TeamScore[(int) Team.Red] >= GameInfo.ScoreLimit || 
                        TeamScore[(int) Team.Blue] >= GameInfo.ScoreLimit) ||
                    GameInfo.TimeLimit > 0 && (Server.Tick - GameStartTick) >= GameInfo.TimeLimit * Server.TickSpeed * 60)
                {
                    if (TeamScore[(int) Team.Red] != TeamScore[(int) Team.Blue] ||
                        GameFlags.HasFlag(GameFlags.Survival))
                        EndMatch();
                    else
                        SuddenDeath = true;

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

                    if (Score(i) > topScore)
                    {
                        topScore = Score(i);
                        topScoreCount = 1;
                    }
                    else if (Score(i) == topScore)
                        topScoreCount++;
                }

                if (GameInfo.ScoreLimit > 0 && topScore >= GameInfo.ScoreLimit ||
                    GameInfo.TimeLimit > 0 && Server.Tick - GameStartTick >= GameInfo.TimeLimit * Server.TickSpeed * 60)
                {
                    if (topScoreCount == 1)
                        EndMatch();
                    else
                        SuddenDeath = true;
                }
            }
        }

        protected override void SetPlayersReadyState(bool state)
        {
            for (var i = 0; i < GameContext.Players.Length; i++)
            {
                if (GameContext.Players[i] == null ||
                    GameContext.Players[i].Team == Team.Spectators ||
                    GameContext.Players[i].DeadSpectatorMode && !state)
                {
                    continue;
                }

                GameContext.Players[i].IsReadyToPlay = state;
            }
        }

        protected override void StartMatch()
        {
            ResetGame();
            RoundCount = 0;
            TeamScore[(int) Team.Red] = 0;
            TeamScore[(int) Team.Blue] = 0;

            if (HasEnoughPlayers())
                SetGameState(GameState.StartCountdown, 0);
            else
                SetGameState(GameState.WarmupGame, TimerInfinite);

            // TODO demo recorder

            Console.Print(OutputLevel.Debug, "game", $"start match={GameType} teamplay={IsTeamplay()}");
        }

        public override Team StartTeam()
        {
            if (Config["SvTournamentMode"])
                return Team.Spectators;

            var team = Team.Red;
            if (IsTeamplay())
            {
                team = TeamSize[(int) Team.Red] > TeamSize[(int) Team.Blue] 
                    ? Team.Blue 
                    : Team.Red;
            }

            if (TeamSize[(int) Team.Red] + TeamSize[(int) Team.Blue] < Config["SvPlayerSlots"])
            {
                TeamSize[(int) team]++;
                UnbalancedTick = BalanceCheck;

                if (GameState == GameState.WarmupGame && HasEnoughPlayers())
                    SetGameState(GameState.WarmupGame, 0);
                return team;
            }

            return Team.Spectators;
        }

        public override bool IsTeamChangeAllowed(BasePlayer player)
        {
            return !World.Paused || GameState == GameState.StartCountdown && GameStartTick == Server.Tick;
        }

        public override bool IsTeamplay()
        {
            return GameFlags.HasFlag(GameFlags.Teams);
        }

        public override bool IsFriendlyFire(int clientId1, int clientId2)
        {
            if (clientId1 == clientId2)
                return false;

            if (IsTeamplay())
            {
                if (GameContext.Players[clientId1] == null ||
                    GameContext.Players[clientId2] == null)
                {
                    return false;
                }

                if (!Config["SvTeamdamage"] && GameContext.Players[clientId1].Team == GameContext.Players[clientId2].Team)
                    return true;
            }

            return false;
        }

        public override bool CanSelfKill(BasePlayer player)
        {
            return true;
        }

        public override bool CanChangeTeam(BasePlayer player, Team team)
        {
            if (!IsTeamplay() || team == Team.Spectators || !Config["SvTeambalanceTime"])
                return true;

            var playerCount = new int[]
            {
                TeamSize[(int) Team.Red],
                TeamSize[(int) Team.Blue],
            };

            playerCount[(int) team]++;

            if (player.Team != Team.Spectators)
                playerCount[(int) team ^ 1]--;

            return playerCount[(int) team] - playerCount[(int) team ^ 1] < 2;
        }

        public override bool CanJoinTeam(BasePlayer player, Team team)
        {
            if (team == Team.Spectators)
                return true;

            var teamMod = player.Team != Team.Spectators ? -1 : 0;
            return teamMod + TeamSize[(int) Team.Red] + TeamSize[(int) Team.Blue] < Config["SvPlayerSlots"];
        }

        public override bool IsPlayerReadyMode()
        {
            return Config["SvPlayerReadyMode"] && GameStateTimer == TimerInfinite && 
                   (GameState == GameState.WarmupUser || GameState == GameState.GamePaused);
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
            if (GamePaused)
            {
                for (var i = 0; i < ScoresStartTick.Length; i++)
                    ScoresStartTick[i]++;
            }

            if (GameState != GameState.GameRunning)
            {
                if (GameStateTimer > 0)
                    GameStateTimer--;

                if (GameStateTimer == 0)
                {
                    switch (GameState)
                    {
                        case GameState.WarmupUser:
                            SetGameState(GameState.WarmupUser, 0);
                            break;

                        case GameState.StartCountdown:
                            SetGameState(GameState.GameRunning, 0);
                            break;

                        case GameState.GamePaused:
                            SetGameState(GameState.GamePaused, 0);
                            break;

                        case GameState.EndRound:
                            StartRound();
                            break;

                        case GameState.EndMatch:
                            if (MatchCount >= GameInfo.MatchNum - 1)
                                CycleMap();

                            if (Config["SvMatchSwap"])
                                SwapTeams();

                            MatchCount++;
                            StartMatch();
                            break;
                    }
                }
                else
                {
                    switch (GameState)
                    {
                        case GameState.WarmupUser:
                            if (!Config["SvPlayerReadyMode"] && GameStateTimer == TimerInfinite)
                                SetGameState(GameState.WarmupUser, 0);
                            else if (GameStateTimer == 3 * Server.TickSpeed)
                                StartMatch();
                            break;

                        case GameState.StartCountdown:
                        case GameState.GamePaused:
                            GameStartTick++;
                            break;
                    }
                }
            }

            if (IsTeamplay() && !GameFlags.HasFlag(GameFlags.Survival))
            {
                switch (UnbalancedTick)
                {
                    case BalanceCheck:
                        CheckTeamBalance();
                        break;
                    case BalanceOk:
                        break;
                    default:
                        if (Server.Tick > UnbalancedTick + Config["SvTeambalanceTime"] * Server.TickSpeed * 60)
                            DoTeamBalance();
                        break;
                }
            }

            DoActivityCheck();

            if (!World.ResetRequested && (
                    GameState == GameState.GameRunning ||
                    GameState == GameState.GamePaused))
            {
                if (GameFlags.HasFlag(GameFlags.Survival))
                    WincheckRound();
                else 
                    WincheckMatch();
            }
        }

        protected override void DoActivityCheck()
        {
            if (!Config["SvInactiveKickTime"])
                return;

            for (var i = 0; i < GameContext.Players.Length; i++)
            {
                if (GameContext.Players[i] == null ||
                    GameContext.Players[i].IsDummy ||
                    GameContext.Players[i].Team == Team.Spectators && Config["SvInactiveKick"] <= 0 ||
                    GameContext.Players[i].InactivityTickCounter <= Config["SvInactiveKickTime"] * Server.TickSpeed * 60 ||
                    Server.IsAuthed(i))
                {
                    continue;
                }

                if (GameContext.Players[i].Team == Team.Spectators)
                    Server.Kick(i, "Kicked for inactivity");
                else
                {
                    switch (Config["SvInactiveKick"].AsInt())
                    {
                        case 0:
                        case 1:
                            GameContext.Players[i].SetTeam(Team.Spectators);
                            break;

                        case 2:
                            var spectators = 0;
                            for (var j = 0; j < GameContext.Players.Length; j++)
                            {
                                if (GameContext.Players[i] != null &&
                                    GameContext.Players[i].Team == Team.Spectators)
                                {
                                    spectators++;
                                }
                            }

                            if (spectators >= GameContext.Players.Length - Config["SvPlayerSlots"])
                                Server.Kick(i, "Kicked for inactivity");
                            else 
                                GameContext.Players[i].SetTeam(Team.Spectators);
                            break;

                        case 3:
                            Server.Kick(i, "Kicked for inactivity");
                            break;
                    }
                }
            }
        }

        protected override void DoTeamBalance()
        {
            if (!IsTeamplay() || !Config["SvTeambalanceTime"] || Math.Abs(TeamSize[(int)Team.Red] - TeamSize[(int)Team.Blue]) < 2)
                return;

            Console.Print(OutputLevel.Debug, "game", "Balancing teams");

            var teamScore = new float[2];
            var playerScore = new float[GameContext.Players.Length];

            for (var i = 0; i < GameContext.Players.Length; i++)
            {
                if (GameContext.Players[i] == null ||
                    GameContext.Players[i].Team == Team.Spectators)
                {
                    continue;
                }

                playerScore[i] = Score(i) * Server.TickSpeed * 60f / (Server.Tick - ScoresStartTick[i]);
                teamScore[(int) GameContext.Players[i].Team] += playerScore[i];
            }

            var biggerTeam = TeamSize[(int) Team.Red] > TeamSize[(int) Team.Blue] 
                ? Team.Red 
                : Team.Blue;
            var numBalance = Math.Abs(TeamSize[(int) Team.Red] - TeamSize[(int) Team.Blue]) / 2;

            do
            {
                var player = default(BasePlayer);
                var scoreDiff = teamScore[(int) biggerTeam];

                for (var i = 0; i < GameContext.Players.Length; i++)
                {
                    if (GameContext.Players[i] == null || !CanBeMovedOnBalance(i))
                        continue;

                    var score = Math.Abs((teamScore[(int) biggerTeam ^ 1] + playerScore[i]) -
                                         (teamScore[(int) biggerTeam] - playerScore[i]));
                    if (GameContext.Players[i].Team == biggerTeam && (player == null || score < scoreDiff))
                    {
                        player = GameContext.Players[i];
                        scoreDiff = score;
                    }
                }

                if (player != null)
                {
                    var tmp = player.LastActionTick;
                    player.SetTeam((Team) ((int) biggerTeam ^ 1));
                    player.LastActionTick = tmp;
                    player.Respawn();
                    GameContext.SendGameplayMessage(player.ClientId, GameplayMessage.TeamBalanceVictim, (int?) player.Team);
                }

            } while (numBalance-- > 0);

            UnbalancedTick = BalanceOk;
            GameContext.SendGameplayMessage(-1, GameplayMessage.TeamBalance);
        }

        protected override bool CanBeMovedOnBalance(int clientId)
        {
            return true;
        }

        protected override void CheckGameInfo()
        {

        }

        protected override void CheckTeamBalance()
        {
            if (!IsTeamplay() || !Config["SvTeambalanceTime"])
            {
                UnbalancedTick = BalanceOk;
                return;
            }

            string message;

            if (Math.Abs(TeamSize[(int)Team.Red] - TeamSize[(int)Team.Blue]) > 2)
            {
                message = $"Teams are NOT balanced (red={TeamSize[(int)Team.Red]} blue={TeamSize[(int)Team.Blue]})";
                if (UnbalancedTick <= BalanceOk)
                    UnbalancedTick = Server.Tick;
            }
            else
            {
                message = $"Teams are balanced (red={TeamSize[(int)Team.Red]} blue={TeamSize[(int)Team.Blue]})";
                UnbalancedTick = BalanceOk;
            }

            Console.Print(OutputLevel.Debug, "game", message);
        }

        protected override void SwapTeams()
        {

        }

        protected override void CycleMap()
        {

        }

        protected override void StartRound()
        {
            ResetGame();
            RoundCount++;

            if (HasEnoughPlayers())
                SetGameState(GameState.StartCountdown, 0);
            else
                SetGameState(GameState.WarmupGame, TimerInfinite);

        }

        public override int Score(int clientId)
        {
            return Scores[clientId];
        }

        public override bool CanSpawn(Team team, int clientId, out Vector2 spawnPos)
        {
            if (team == Team.Spectators || World.Paused)
            {
                spawnPos = Vector2.Zero;
                return false;
            }

            var eval = new SpawnEval();
            if (IsTeamplay())
            {
                eval.FriendlyTeam = team;

                EvaluateSpawnType(eval, SpawnPos[1 + ((int)team & 1)]);
                if (!eval.Got)
                {
                    EvaluateSpawnType(eval, SpawnPos[0]);
                    if (!eval.Got)
                        EvaluateSpawnType(eval, SpawnPos[1 + (((int)team + 1) & 1)]);
                }
            }
            else
            {
                EvaluateSpawnType(eval, SpawnPos[0]);
                EvaluateSpawnType(eval, SpawnPos[1]);
                EvaluateSpawnType(eval, SpawnPos[2]);
            }

            spawnPos = eval.Position;
            return eval.Got;
        }

        protected virtual float EvaluateSpawnPos(SpawnEval eval, Vector2 pos)
        {
            var score = 0f;

            foreach (var character in Character.Entities)
            {
                var scoremod = 1f;
                if (eval.FriendlyTeam != Team.Spectators && character.Player.Team == eval.FriendlyTeam)
                    scoremod = 0.5f;

                var d = MathHelper.Distance(pos, character.Position);
                score += scoremod * (System.Math.Abs(d) < 0.00001 ? 1000000000.0f : 1.0f / d);
            }

            return score;
        }

        protected virtual void EvaluateSpawnType(SpawnEval eval, IList<Vector2> spawnPos)
        {
            for (var i = 0; i < spawnPos.Count; i++)
            {
                var positions = new[]
                {
                    new Vector2(0.0f, 0.0f),
                    new Vector2(-32.0f, 0.0f),
                    new Vector2(0.0f, -32.0f),
                    new Vector2(32.0f, 0.0f),
                    new Vector2(0.0f, 32.0f)
                };  // start, left, up, right, down

                var result = -1;
                for (var index = 0; index < 5 && result == -1; ++index)
                {
                    result = index;

                    foreach (var character in Character.Entities.Find(spawnPos[i], 64f))
                    {
                        if (GameContext.MapCollision.IsTileSolid(spawnPos[i] + positions[index]) ||
                            MathHelper.Distance(character.Position, spawnPos[i] + positions[index]) <=
                            character.ProximityRadius)
                        {
                            result = -1;
                            break;
                        }
                    }

                    if (result == -1)
                        continue;

                    var p = spawnPos[i] + positions[index];
                    var s = EvaluateSpawnPos(eval, p);

                    if (!eval.Got || eval.Score > s)
                    {
                        eval.Got = true;
                        eval.Score = s;
                        eval.Position = p;
                    }
                }
            }
        }

        public override void OnPlayerChat(BasePlayer player, GameMsg_ClSay message, out bool isSend)
        {
            isSend = true;
        }

        public override void OnPlayerInfoChange(BasePlayer player)
        {
            
        }

        protected override void OnPlayerReady(BasePlayer player)
        {
            player.CharacterSpawned += OnCharacterSpawn;
            player.TeamChanged += OnPlayerTeamChanged;

            ScoresStartTick[player.ClientId] = Server.Tick;
            Scores[player.ClientId] = 0;
        }

        protected override void OnPlayerEnter(BasePlayer player)
        {
            player.Respawn();
            Console.Print(OutputLevel.Debug, "game",
                $"team_join player='{player.ClientId}:{Server.ClientName(player.ClientId)}' team={player.Team}");
            UpdateGameInfo(player.ClientId);
        }

        protected override void OnPlayerTeamChanged(BasePlayer player, Team prevTeam, Team newTeam)
        {
            Console.Print(OutputLevel.Debug, "game", $"team join player={player.ClientId}:{Server.ClientName(player.ClientId)} team={newTeam}");

            if (prevTeam != Team.Spectators || newTeam != Team.Spectators)
            {
                UnbalancedTick = BalanceCheck;
            }

            if (prevTeam != Team.Spectators)
            {
                TeamSize[(int) prevTeam]--;
            }

            if (newTeam != Team.Spectators)
            {
                TeamSize[(int)newTeam]++;

                if (GameState == GameState.WarmupGame && HasEnoughPlayers())
                    SetGameState(GameState.WarmupGame, 0);

                player.IsReadyToPlay = !IsPlayerReadyMode();
                if (GameFlags.HasFlag(GameFlags.Survival))
                    player.RespawnDisabled = GetRespawnDisabled(player);
            }

            CheckReadyStates();
        }

        protected override bool HasEnoughPlayers()
        {
            return true;
        }

        protected override void OnCharacterSpawn(BasePlayer player, Character character)
        {
            character.Died += OnCharacterDied;

            if (GameFlags.HasFlag(GameFlags.Survival))
            {
                character.IncreaseHealth(10);
                character.IncreaseArmor(5);

                character.GiveWeapon(Weapon.Gun, 10);
                character.GiveWeapon(Weapon.Grenade, 10);
                character.GiveWeapon(Weapon.Shotgun, 10);
                character.GiveWeapon(Weapon.Laser, 5);

                player.RespawnDisabled = GetRespawnDisabled(player);
            }
            else
            {
                character.IncreaseHealth(10);
                character.GiveWeapon(Weapon.Gun, 10);
            }
        }

        protected override void OnCharacterDied(Character victim, BasePlayer killer, Weapon weapon, ref int modespecial)
        {
            victim.Died -= OnCharacterDied;

            if (killer == null || weapon == BasePlayer.WeaponGame)
                return;

            if (killer == victim.Player)
                Scores[killer.ClientId]--;
            else
            {
                if (IsTeamplay() && victim.Player.Team == killer.Team)
                    Scores[killer.ClientId]--;
                else
                    Scores[killer.ClientId]++;
            }

            if (weapon == BasePlayer.WeaponSelf)
                victim.Player.RespawnTick = Server.Tick + Server.TickSpeed * 3;

            if (GameFlags.HasFlag(GameFlags.Survival))
            {
                for (var i = 0; i < GameContext.Players.Length; i++)
                {
                    if (GameContext.Players[i] != null && GameContext.Players[i].DeadSpectatorMode)
                        GameContext.Players[i].UpdateDeadSpecMode();
                }
            }
        }

        public override void OnPlayerReadyChange(BasePlayer player)
        {
            if (Config["SvPlayerReadyMode"] && player.Team != Team.Spectators && !player.DeadSpectatorMode)
            {
                player.IsReadyToPlay ^= true;

                if (GameState == GameState.GameRunning && !player.IsReadyToPlay)
                    SetGameState(GameState.GamePaused, TimerInfinite);

                CheckReadyStates();
            }   
        }
        
        public override void OnEntity(Tile tile, Vector2 pos)
        {
            if (tile.Index < (int) MapEntities.EntityOffset)
                return;

            var entity = tile.Index - MapEntities.EntityOffset;
            Pickup? pickup = null;

            switch (entity)
            {
                case MapEntities.Spawn:
                    SpawnPos[0].Add(pos);
                    break;
                case MapEntities.SpawnRed:
                    SpawnPos[1].Add(pos);
                    break;
                case MapEntities.SpawnBlue:
                    SpawnPos[2].Add(pos);
                    break;
                case MapEntities.Armor:
                    pickup = Pickup.Armor;
                    break;
                case MapEntities.Health:
                    pickup = Pickup.Health;
                    break;
                case MapEntities.WeaponShotgun:
                    pickup = Pickup.Shotgun;
                    break;
                case MapEntities.WeaponGrenade:
                    pickup = Pickup.Grenade;
                    break;
                case MapEntities.PowerupNinja:
                    pickup = Pickup.Ninja;
                    break;
                case MapEntities.WeaponLaser:
                    pickup = Pickup.Laser;
                    break;
            }

            if (pickup.HasValue)
            {
                new Entities.Pickup(pickup.Value).Position = pos;
            }
        }

        protected override bool GetPlayersReadyState(int withoutID = -1)
        {
            for (var i = 0; i < GameContext.Players.Length; i++)
            {
                if (i == withoutID)
                    continue;

                if (GameContext.Players[i] != null &&
                    GameContext.Players[i].Team != Team.Spectators &&
                    GameContext.Players[i].IsReadyToPlay == false)
                {
                    return false;
                }
            }

            return true;
        }

        protected override void CheckReadyStates(int withoutID = -1)
        {
            if (!Config["SvPlayerReadyMode"])
                return;

            switch (GameState)
            {
                case GameState.WarmupUser:
                    if (GetPlayersReadyState(withoutID))
                        SetGameState(GameState.WarmupUser, 0);
                    break;

                case GameState.GamePaused:
                    if (GetPlayersReadyState(withoutID))
                        SetGameState(GameState.GamePaused, 0);
                    break;
            }
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
        
        public override void OnSnapshot(int snappingId, out SnapshotGameData gameData)
        {
            gameData = Server.SnapshotItem<SnapshotGameData>(0);
            if (gameData == null)
                return;
            
            gameData.GameStartTick = GameStartTick; 
            gameData.GameStateFlags = GameStateFlags.None;
            gameData.GameStateEndTick = 0;

            if (GameState == GameState.WarmupGame ||
                GameState == GameState.WarmupUser ||
                GameState == GameState.StartCountdown ||
                GameState == GameState.GamePaused)
            {
                if (GameStateTimer != TimerInfinite)
                    gameData.GameStateEndTick = Server.Tick + GameStateTimer;
            }

            switch (GameState)
            {
                case GameState.WarmupGame:
                case GameState.WarmupUser:
                    gameData.GameStateFlags |= GameStateFlags.Warmup;
                    break;

                case GameState.StartCountdown:
                    gameData.GameStateFlags |= GameStateFlags.StartCountDown | GameStateFlags.Paused;
                    break;

                case GameState.GamePaused:
                    gameData.GameStateFlags |= GameStateFlags.Paused;
                    break;

                case GameState.EndRound:
                    gameData.GameStateFlags |= GameStateFlags.RoundOver;
                    gameData.GameStateEndTick = Server.Tick - GameStartTick - TimerEnd / 2 * Server.TickSpeed + GameStateTimer;
                    break;

                case GameState.EndMatch:
                    gameData.GameStateFlags |= GameStateFlags.GameOver;
                    gameData.GameStateEndTick = Server.Tick - GameStartTick - TimerEnd * Server.TickSpeed + GameStateTimer;
                    break;
            }

            if (SuddenDeath)
                gameData.GameStateFlags |= GameStateFlags.SuddenDeath;

            if (IsTeamplay())
            {
                var gameDataTeam = Server.SnapshotItem<SnapshotGameDataTeam>(0);
                if (gameDataTeam != null)
                {
                    gameDataTeam.ScoreBlue = TeamScore[(int) Team.Blue];
                    gameDataTeam.ScoreRed = TeamScore[(int) Team.Red];
                }
            }

            if (snappingId == -1)
            {
                var gameInfo = Server.SnapshotItem<SnapshotDemoGameInfo>(0);
                if (gameInfo != null)
                {
                    gameInfo.GameFlags = GameFlags;
                    gameInfo.ScoreLimit = GameInfo.ScoreLimit;
                    gameInfo.TimeLimit = GameInfo.TimeLimit;
                    gameInfo.MatchNum = GameInfo.MatchNum;
                    gameInfo.MatchCurrent = GameInfo.MatchCurrent;
                }
            }
        }
    }
}
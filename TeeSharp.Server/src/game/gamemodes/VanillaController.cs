using System;
using System.Collections.Generic;
using System.Linq;
using TeeSharp.Common;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;
using TeeSharp.Server.Game.Entities;
using Math = System.Math;

namespace TeeSharp.Server.Game
{
    public abstract class VanillaController : BaseGameController
    {
        protected virtual IList<vec2>[] SpawnPos { get; set; }

        protected VanillaController()
        {
            SpawnPos = new IList<vec2>[]
            {
                new List<vec2>(), // default spawn pos
                new List<vec2>(), // red team spawn pos
                new List<vec2>()  // blue team spawn pos
            };

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

        protected override float EvaluateSpawnPos(SpawnEval eval, vec2 pos)
        {
            var score = 0f;

            foreach (var character in GameContext.World.GetEntities<Character>())
            {
                var scoremod = 1f;
                if (eval.FriendlyTeam != Team.SPECTATORS && character.Player.Team == eval.FriendlyTeam)
                    scoremod = 0.5f;

                var d = VectorMath.Distance(pos, character.Position);
                score += scoremod * (Math.Abs(d) < 0.00001 ? 1000000000.0f : 1.0f / d);
            }

            return score;
        }

        protected override void EvaluateSpawnType(SpawnEval eval, IList<vec2> spawnPos)
        {
            for (var i = 0; i < spawnPos.Count; i++)
            {
                var positions = new[]
                {
                    new vec2(0.0f, 0.0f),
                    new vec2(-32.0f, 0.0f),
                    new vec2(0.0f, -32.0f),
                    new vec2(32.0f, 0.0f),
                    new vec2(0.0f, 32.0f)
                };  // start, left, up, right, down

                var result = -1;
                var characters = GameContext.World
                    .FindEntities<Character>(spawnPos[i], 64f)
                    .ToArray();

                for (var index = 0; index < 5 && result == -1; ++index)
                {
                    result = index;

                    for (var c = 0; c < characters.Length; c++)
                    {
                        if (GameContext.Collision.IsTileSolid(spawnPos[i] + positions[index]) ||
                            VectorMath.Distance(characters[c].Position, spawnPos[i] + positions[index]) <=
                            characters[c].ProximityRadius)
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
                        eval.Pos = p;
                    }
                }
            }
        }

        public override bool CanSpawn(Team team, int clientId, out vec2 spawnPos)
        {
            if (team == Team.SPECTATORS)
            {
                spawnPos = vec2.zero;
                return false;
            }

            var eval = new SpawnEval();

            if (IsTeamplay())
            {
                eval.FriendlyTeam = team;

                EvaluateSpawnType(eval, SpawnPos[1 + ((int) team & 1)]);
                if (!eval.Got)
                {
                    EvaluateSpawnType(eval, SpawnPos[0]);
                    if (!eval.Got)
                        EvaluateSpawnType(eval, SpawnPos[1 + (((int) team + 1) & 1)]);
                }
            }
            else
            {
                EvaluateSpawnType(eval, SpawnPos[0]);
                EvaluateSpawnType(eval, SpawnPos[1]);
                EvaluateSpawnType(eval, SpawnPos[2]);
            }

            spawnPos = eval.Pos;
            return eval.Got;
        }
        
        public override bool CanChangeTeam(BasePlayer player, Team joinTeam)
        {
            if (!IsTeamplay() || joinTeam == Team.SPECTATORS || Config["SvTeambalanceTime"])
                return true;

            var aT = new int[] {0, 0};

            for (var i = 0; i < GameContext.Players.Length; i++)
            {
                if (GameContext.Players[i] == null)
                    continue;

                if (GameContext.Players[i].Team != Team.SPECTATORS)
                    aT[(int) GameContext.Players[i].Team]++;
            }

            aT[(int) joinTeam]++;

            if (player.Team != Team.SPECTATORS)
                aT[(int) joinTeam ^ 1]--;

            if (Math.Abs(aT[0] - aT[1]) >= 2)
            {
                return aT[0] < aT[1] && joinTeam == Team.RED ||
                       aT[0] > aT[1] && joinTeam == Team.BLUE;
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

                if (GameContext.Players[i].Team >= Team.RED &&
                    GameContext.Players[i].Team <= Team.BLUE)
                {
                    numPlayers[(int)GameContext.Players[i].Team]++;
                }
            }

            var team = (Team) 0;
            if (IsTeamplay())
            {
                team = numPlayers[(int) Team.RED] > numPlayers[(int) Team.BLUE]
                    ? Team.BLUE
                    : Team.RED;
            }

            return CanJoinTeam(clientId, team) 
                ? team 
                : Team.SPECTATORS;
        }

        public override bool CanJoinTeam(int clientId, Team team)
        {
            if (team == Team.SPECTATORS ||
                GameContext.Players[clientId] != null &&
                GameContext.Players[clientId].Team != Team.SPECTATORS)
            {
                return true;
            }

            var numPlayers = new[] {0, 0};
            for (var i = 0; i < GameContext.Players.Length; i++)
            {
                if (GameContext.Players[i] == null || clientId == i) 
                    continue;

                if (GameContext.Players[i].Team >= Team.RED &&
                    GameContext.Players[i].Team <= Team.BLUE)
                {
                    numPlayers[(int) GameContext.Players[i].Team]++;
                }
            }

            return numPlayers[0] + numPlayers[1] < Server.MaxClients - Config["SvSpectatorSlots"];
        }
        
        public override bool IsTeamplay()
        {
            return GameFlags.HasFlag(GameFlags.TEAMS);
        }

        public override Team ClampTeam(Team team)
        {
            if (team < 0)
                return Team.SPECTATORS;
            if (IsTeamplay())
                return (Team) ((int) team & 1);
            return 0;
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

        public override bool CheckTeamsBalance()
        {
            return true;
        }

        public override void OnCharacterSpawn(Character character)
        {
        }

        public override void OnEntity(int entityIndex, vec2 pos)
        {
            var item = (MapItems) entityIndex;
            var powerup = Powerup.NONE;
            var weapon = Weapon.HAMMER;

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
                    powerup = Powerup.ARMOR;
                    break;

                case MapItems.ENTITY_HEALTH_1:
                    powerup = Powerup.HEALTH;
                    break;

                case MapItems.ENTITY_WEAPON_SHOTGUN:
                    powerup = Powerup.WEAPON;
                    weapon = Weapon.SHOTGUN;
                    break;

                case MapItems.ENTITY_WEAPON_GRENADE:
                    powerup = Powerup.WEAPON;
                    weapon = Weapon.GRENADE;
                    break;

                case MapItems.ENTITY_POWERUP_NINJA:
                    powerup = Powerup.NINJA;
                    weapon = Weapon.NINJA;
                    break;

                case MapItems.ENTITY_WEAPON_RIFLE:
                    powerup = Powerup.WEAPON;
                    weapon = Weapon.RIFLE;
                    break;
            }

            if (powerup != Powerup.NONE)
            {
                var pickup = new Pickup(powerup, weapon)
                {
                    Position = pos
                };
            }
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
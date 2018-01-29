using TeeSharp.Common;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;
using TeeSharp.Server.Game.Entities;

namespace TeeSharp.Server.Game
{
    public abstract class VanillaController : BaseGameController
    {
        protected VanillaController()
        {
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

        public override void OnEntity(int entityIndex, vec2 pos)
        {
            var item = (MapItems) entityIndex;
            var powerup = Powerup.NONE;
            var weapon = Weapon.HAMMER;

            switch (item)
            {
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
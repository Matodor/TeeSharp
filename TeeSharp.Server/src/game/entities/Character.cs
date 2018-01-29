using TeeSharp.Common;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Game;
using TeeSharp.Common.Protocol;

namespace TeeSharp.Server.Game.Entities
{
    public class Character : Entity<Character>
    {
        public override float ProximityRadius { get; protected set; } = 28f;

        public virtual int Health { get; protected set; }
        public virtual int Armor { get; protected set; }

        public virtual bool IsAlive { get; protected set; }
        public virtual BasePlayer Player { get; protected set; }

        protected virtual CharacterCore Core { get; set; }
        protected virtual CharacterCore SendCore { get; set; }
        protected virtual CharacterCore ReckoningCore { get; set; }

        protected virtual int NumInputs { get; set; }
        protected virtual int LastAction { get; set; }
        protected virtual int EmoteStopTick { get; set; }
        protected virtual int ReckoningTick { get; set; }
        protected virtual int AttackTick { get; set; }
        protected virtual Emote Emote { get; set; }
        protected virtual Weapon ActiveWeapon { get; set; }

        protected virtual SnapObj_PlayerInput Input { get; set; }
        protected virtual SnapObj_PlayerInput LatestPrevInput { get; set; }
        protected virtual SnapObj_PlayerInput LatestInput { get; set; }

        public Character(BasePlayer player, Vec2 spawnPos) : base(1)
        {
            ActiveWeapon = Weapon.HAMMER;
            Position = spawnPos;
            Health = 0;
            Armor = 0;
            EmoteStopTick = -1;
            LastAction = -1;
            Player = player;
            IsAlive = true;

            Input = new SnapObj_PlayerInput();
            LatestPrevInput = new SnapObj_PlayerInput();
            LatestInput = new SnapObj_PlayerInput();

            Core = new CharacterCore();
            SendCore = new CharacterCore();
            ReckoningCore = new CharacterCore();

            Core.Init(GameWorld.WorldCore);
            Core.Position = Position;
            Core.Input = Input;

            GameWorld.WorldCore.CharacterCores[player.ClientId] = Core;
            GameContext.GameController.OnCharacterSpawn(this);
        }

        public override void OnDestroy()
        {
            GameWorld.WorldCore.CharacterCores[Player.ClientId] = null;
            IsAlive = false;
            base.OnDestroy();
        }

        public virtual void SetEmote(Emote emote, int stopTick)
        {
            Emote = emote;
            EmoteStopTick = stopTick;
        }

        public virtual void Die(int clientId, Weapon weapon)
        {
        }

        public virtual  void OnPredictedInput(SnapObj_PlayerInput newInput)
        {
            if (!Input.Compare(newInput))
                LastAction = Server.Tick;

            Input.FillFrom(newInput);
            NumInputs++;

            if (Input.TargetX == 0 && Input.TargetY == 0)
                Input.TargetY = -1;
        }

        public virtual void OnDirectInput(SnapObj_PlayerInput newInput)
        {
            LatestPrevInput.FillFrom(LatestInput);
            LatestInput.FillFrom(newInput);

            if (LatestInput.TargetX == 0 && LatestInput.TargetY == 0)
                LatestInput.TargetY = -1;

            if (NumInputs > 2 && Player.Team != Team.SPECTATORS)
            {
                HandleWeaponSwitch();
                FireWeapon();
            }

            LatestPrevInput.FillFrom(LatestInput);
        }

        protected virtual void HandleWeaponSwitch()
        {
            
        }

        protected virtual void FireWeapon()
        {
            
        }

        protected virtual void HandleWeapons()
        {
            
        }

        public virtual void ResetInput()
        {
            Input.Direction = 0;
            Input.Hook = 0;

            if ((Input.Fire & 1) != 0)
                Input.Fire++;

            Input.Fire &= SnapObj_PlayerInput.INPUT_STATE_MASK;
            Input.Jump = 0;
            
            LatestInput.FillFrom(Input);
            LatestPrevInput.FillFrom(Input);
        }

        public override void Tick()
        {
            Core.Tick(true);

            var rDiv3 = ProximityRadius / 3.0f;

            if (GameContext.Collision.GetTileFlags(Position.x + rDiv3, Position.y - rDiv3).HasFlag(TileFlags.DEATH) ||
                GameContext.Collision.GetTileFlags(Position.x + rDiv3, Position.y + rDiv3).HasFlag(TileFlags.DEATH) ||
                GameContext.Collision.GetTileFlags(Position.x - rDiv3, Position.y - rDiv3).HasFlag(TileFlags.DEATH) ||
                GameContext.Collision.GetTileFlags(Position.x - rDiv3, Position.y + rDiv3).HasFlag(TileFlags.DEATH) ||
                GameLayerClipped(Position))
            {
                Die(Player.ClientId, Weapon.WORLD);
            }

            HandleWeapons();
        }

        public override void TickDefered()
        {
            {
                var worldCore = new WorldCore(
                    GameWorld.WorldCore.CharacterCores.Length,
                    GameWorld.WorldCore.Tuning);

                ReckoningCore.Init(worldCore);
                ReckoningCore.Tick(false);
                ReckoningCore.Move();
                ReckoningCore.Quantize();
            }

            Core.Move();
            Core.Quantize();
            Position = Core.Position;

            if (Player.Team == Team.SPECTATORS)
                Position = new Vec2(Input.TargetX, Input.TargetY);


            {
                var predicted = new SnapObj_Character();
                var current = new SnapObj_Character();

                ReckoningCore.Write(predicted);
                Core.Write(current);

                if (ReckoningTick + Server.TickSpeed * 3 < Server.Tick || !predicted.Compare(current))
                {
                    ReckoningTick = Server.Tick;
                    Core.FillTo(SendCore);
                    Core.FillTo(ReckoningCore);
                }
            }
        }

        public override void TickPaused()
        {
            AttackTick++;
            ReckoningTick++;

            if (LastAction != -1)
                LastAction++;

            if (EmoteStopTick > -1)
                EmoteStopTick++;
        }

        public override void OnSnapshot(int snappingClient)
        {
            var id = Player.ClientId;

            if (!Server.Translate(ref id, snappingClient))
                return;

            if (NetworkClipped(snappingClient))
                return;

            var character = Server.SnapObject<SnapObj_Character>(id);
            if (character == null)
                return;

            if (ReckoningTick == 0 || GameWorld.IsPaused)
            {
                character.Tick = 0;
                Core.Write(character);
            }
            else
            {
                character.Tick = ReckoningTick;
                SendCore.Write(character);
            }

            if (EmoteStopTick < Server.Tick)
            {
                EmoteStopTick = -1;
                Emote = Emote.NORMAL;
            }

            character.Emote = Emote;
            character.AmmoCount = 0;
            character.Health = 0;
            character.Armor = 0;

            character.Weapon = ActiveWeapon;
            character.AttackTick = AttackTick;
            character.Direction = Input.Direction;

            if (snappingClient == Player.ClientId || snappingClient == -1 ||
                !Config["SvStrictSpectateMode"] && Player.ClientId == GameContext.Players[snappingClient].SpectatorId)
            {
                character.Health = Health;
                character.Armor = Armor;


            }

            if (character.Emote == Emote.NORMAL)
            {
                if (250 - ((Server.Tick - LastAction) % 250) < 5)
                    character.Emote = Emote.BLINK;
            }

            character.PlayerFlags = Player.PlayerFlags;
        }
    }
}
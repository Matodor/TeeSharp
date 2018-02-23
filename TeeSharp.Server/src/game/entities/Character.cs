using System.Collections.Generic;
using TeeSharp.Common;
using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Game;
using TeeSharp.Common.Protocol;

namespace TeeSharp.Server.Game.Entities
{
    public struct WeaponStat
    {
        public int AmmoRegenStart;
        public int Ammo;
        public int AmmoCost;
        public bool Got;
    }

    public class NinjaStat
    {
        public Vec2 ActivationDir;
        public int ActivationTick;
        public int CurrentMoveTime;
        public float OldVelAmount;
    }

    public struct InputCount
    {
        public int Presses;
        public int Releases;

        public static InputCount Count(int prev, int cur)
        {
            var c = new InputCount() {Presses = 0, Releases = 0};
            prev &= SnapObj_PlayerInput.INPUT_STATE_MASK;
            cur &= SnapObj_PlayerInput.INPUT_STATE_MASK;
            var i = prev;

            while (i != cur)
            {
                i = (i + 1) & SnapObj_PlayerInput.INPUT_STATE_MASK;
                if ((i & 1) != 0)
                    c.Presses++;
                else
                    c.Releases++;
            }

            return c;
        }
    }

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
        protected virtual int ReloadTimer { get; set; }
        protected virtual int LastNoAmmoSound { get; set; }
        protected virtual int DamageTaken { get; set; }
        protected virtual int DamageTakenTick { get; set; }
        protected virtual Emote Emote { get; set; }
        protected virtual Weapon ActiveWeapon { get; set; }
        protected virtual Weapon LastWeapon { get; set; }
        protected virtual Weapon QueuedWeapon { get; set; }

        protected virtual SnapObj_PlayerInput Input { get; set; }
        protected virtual SnapObj_PlayerInput LatestPrevInput { get; set; }
        protected virtual SnapObj_PlayerInput LatestInput { get; set; }

        protected virtual NinjaStat NinjaStat { get; set; }
        protected virtual WeaponStat[] Weapons { get; set; }
        protected virtual IList<Entity> HitObjects { get; set; }

        public Character(BasePlayer player, Vec2 spawnPos) : base(1)
        {
            HitObjects = new List<Entity>();
            Weapons = new WeaponStat[(int) Weapon.NUM_WEAPONS];
            NinjaStat = new NinjaStat();

            ActiveWeapon = Weapon.GUN;
            LastWeapon = Weapon.HAMMER;
            QueuedWeapon = (Weapon) (-1);

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

            Core.Reset();
            Core.Init(GameWorld.WorldCore, GameContext.Collision);
            Core.Position = Position;

            var worldCore = new WorldCore(
                GameWorld.WorldCore.CharacterCores.Length,
                GameWorld.WorldCore.Tuning);
            ReckoningCore.Init(worldCore, GameContext.Collision);

            GameWorld.WorldCore.CharacterCores[player.ClientId] = Core;
            GameContext.GameController.OnCharacterSpawn(this);
        }
        
        public override void OnDestroy()
        {
            GameWorld.WorldCore.CharacterCores[Player.ClientId] = null;
            IsAlive = false;
            base.OnDestroy();
        }

        public virtual void SetWeapon(Weapon weapon)
        {
            if (weapon == ActiveWeapon)
                return;

            LastWeapon = ActiveWeapon;
            QueuedWeapon = (Weapon) (-1);
            ActiveWeapon = weapon;
            GameContext.CreateSound(Position, Sound.WEAPON_SWITCH);

            if (ActiveWeapon < 0 || ActiveWeapon >= Weapon.NUM_WEAPONS)
                ActiveWeapon = Weapon.HAMMER;
        }

        public bool IsGrounded()
        {
            if (GameContext.Collision.IsTileSolid(
                Position.x + ProximityRadius / 2,
                Position.y + ProximityRadius / 2 + 2))
            {
                return true;
            }

            if (GameContext.Collision.IsTileSolid(
                Position.x - ProximityRadius / 2,
                Position.y + ProximityRadius / 2 + 2))
            {
                return true;
            }

            return false;
        }

        public virtual void SetEmote(Emote emote, int stopTick)
        {
            Emote = emote;
            EmoteStopTick = stopTick;
        }

        public virtual void Die(int killer, Weapon weapon)
        {
            Player.RespawnTick = Server.Tick + Server.TickSpeed / 2;
            var modeSpecial = GameContext.GameController.OnCharacterDeath(this,
                GameContext.Players[killer], weapon);

            GameContext.Console.Print(OutputLevel.DEBUG, "game",
                $"kill killer='{killer}:{GameContext.Players[killer].Name}' victim='{Player.ClientId}:{Player.Name}' weapon={weapon} special={modeSpecial}");

            Server.SendPackMsg(new GameMsg_SvKillMsg
            {
                Killer = killer,
                Victim = Player.ClientId,
                Weapon = weapon,
                ModeSpecial = modeSpecial
            }, MsgFlags.VITAL, -1);

            GameContext.CreateSound(Position, Sound.PLAYER_DIE);
            Player.DieTick = Server.Tick;
            IsAlive = false;
            GameContext.CreateDeath(Position, Player.ClientId);
            Destroy();
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

        protected virtual void DoWeaponSwitch()
        {
            if (ReloadTimer != 0 || QueuedWeapon == (Weapon) (-1) || Weapons[(int) Weapon.NINJA].Got)
                return;

            SetWeapon(QueuedWeapon);
        }

        protected virtual void HandleWeaponSwitch()
        {
            var wantedWeapon = ActiveWeapon;
            if (QueuedWeapon != (Weapon) (-1))
                wantedWeapon = QueuedWeapon;

            var next = InputCount.Count(LatestPrevInput.NextWeapon, LatestInput.NextWeapon).Presses;
            var prev = InputCount.Count(LatestPrevInput.PrevWeapon, LatestInput.PrevWeapon).Presses;

            if (next < 128)
            {
                while (next != 0)
                {
                    wantedWeapon = (Weapon) ((int) (wantedWeapon + 1) % (int) Weapon.NUM_WEAPONS);
                    if (Weapons[(int) wantedWeapon].Got)
                        next--;
                }
            }

            if (prev < 128)
            {
                while (prev != 0)
                {
                    wantedWeapon = wantedWeapon - 1 < 0
                        ? Weapon.NUM_WEAPONS - 1
                        : wantedWeapon - 1;
                    if (Weapons[(int) wantedWeapon].Got)
                        prev--;
                }
            }

            if (LatestInput.WantedWeapon != 0)
                wantedWeapon = (Weapon) (Input.WantedWeapon - 1);

            if (wantedWeapon >= 0 &&
                wantedWeapon < Weapon.NUM_WEAPONS &&
                wantedWeapon != ActiveWeapon &&
                Weapons[(int) wantedWeapon].Got)
            {
                QueuedWeapon = wantedWeapon;
            }

            DoWeaponSwitch();
        }

        protected virtual bool CanFire()
        {
            var fullAuto = ActiveWeapon == Weapon.GRENADE ||
                           ActiveWeapon == Weapon.SHOTGUN ||
                           ActiveWeapon == Weapon.RIFLE;

            var willFire = InputCount.Count(LatestPrevInput.Fire, LatestInput.Fire).Presses != 0 ||
                           fullAuto && (LatestInput.Fire & 1) != 0 && Weapons[(int)ActiveWeapon].Ammo != 0;

            if (!willFire)
                return false;

            if (Weapons[(int)ActiveWeapon].Ammo == 0)
            {
                ReloadTimer = 125 * Server.TickSpeed / 1000;
                if (LastNoAmmoSound + Server.TickSpeed <= Server.Tick)
                {
                    GameContext.CreateSound(Position, Sound.WEAPON_NOAMMO);
                    LastNoAmmoSound = Server.Tick;
                }
                return false;
            }

            return true;
        }

        protected virtual void DoWeaponFireNinja(Vec2 projStartPos, Vec2 direction)
        {
            HitObjects.Clear();
            NinjaStat.ActivationDir = direction;
            NinjaStat.CurrentMoveTime = ServerData.Data.Weapons.Ninja.MoveTime * Server.TickSpeed / 1000;
            NinjaStat.OldVelAmount = Core.Velocity.Length;

            GameContext.CreateSound(Position, Sound.NINJA_FIRE);
        }

        protected virtual void DoWeaponFireGrenade(Vec2 projStartPos, Vec2 direction)
        {
            var projectile = new Projectile(Weapon.GRENADE, Player.ClientId,
                projStartPos, direction, (int) (Server.TickSpeed * Tuning["GrenadeLifetime"]),
                1, true, 0f, Sound.GRENADE_EXPLODE);

            var msg = new MsgPacker((int)GameMessages.SV_EXTRAPROJECTILE);
            msg.AddInt(1);

            var snapObj = new SnapObj_Projectile();
            projectile.FillInfo(snapObj);
            snapObj.FillMsgPacker(msg);

            Server.SendMsg(msg, MsgFlags.NONE, Player.ClientId);
            GameContext.CreateSound(Position, Sound.GRENADE_FIRE);
        }

        protected virtual void DoWeaponFireShotgun(Vec2 projStartPos, Vec2 direction)
        {
            const int SHOT_SPREAD = 2;

            var spreading = new[] { -0.185f, -0.070f, 0, 0.070f, 0.185f };
            var msg = new MsgPacker((int)GameMessages.SV_EXTRAPROJECTILE);
            msg.AddInt(SHOT_SPREAD * 2 + 1);

            for (var i = -SHOT_SPREAD; i <= SHOT_SPREAD; i++)
            {
                var angle = Math.GetAngle(direction);
                angle += spreading[i + 2];
                var v = 1 - System.Math.Abs(i) / (float)SHOT_SPREAD;
                var speed = Math.Mix(Tuning["ShotgunSpeeddiff"], 1f, v);

                var projectile = new Projectile(Weapon.SHOTGUN, Player.ClientId,
                    projStartPos,
                    new Vec2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * speed,
                    (int)(Server.TickSpeed * Tuning["ShotgunLifetime"]), 1, false,
                    0, (Sound)(-1));

                var snapObj = new SnapObj_Projectile();
                projectile.FillInfo(snapObj);
                snapObj.FillMsgPacker(msg);
            }

            Server.SendMsg(msg, MsgFlags.NONE, Player.ClientId);
            GameContext.CreateSound(Position, Sound.SHOTGUN_FIRE);
        }

        protected virtual void DoWeaponFireGun(Vec2 projStartPos, Vec2 direction)
        {
            var projectile = new Projectile(Weapon.GUN, Player.ClientId,
                projStartPos, direction, (int)(Server.TickSpeed * Tuning["GunLifetime"]),
                1, false, 0f, (Sound)(-1));

            var snapObj = new SnapObj_Projectile();
            var msg = new MsgPacker((int)GameMessages.SV_EXTRAPROJECTILE);
            msg.AddInt(1);

            projectile.FillInfo(snapObj);
            snapObj.FillMsgPacker(msg);

            Server.SendMsg(msg, MsgFlags.NONE, Player.ClientId);
            GameContext.CreateSound(Position, Sound.GUN_FIRE);
        }

        protected virtual void DoWeaponFireHammer(Vec2 projStartPos, Vec2 direction)
        {
            GameContext.CreateSound(Position, Sound.HAMMER_FIRE);
            var targets = GameWorld.FindEntities<Character>(projStartPos, ProximityRadius * 0.5f);
            var hits = 0;

            foreach (var target in targets)
            {
                if (target == this || GameContext.Collision.IntersectLine(
                        projStartPos, target.Position, out var _, out var _) != TileFlags.NONE)
                {
                    continue;
                }

                if ((target.Position - projStartPos).Length > 0)
                {
                    GameContext.CreateHammerHit(
                        target.Position - (target.Position - projStartPos)
                        .Normalized * ProximityRadius * 0.5f);
                }
                else
                    GameContext.CreateHammerHit(projStartPos);

                var dir = (target.Position - Position).Length > 0
                    ? (target.Position - Position).Normalized
                    : new Vec2(0, -1f);

                target.TakeDamage(new Vec2(0f, -1f) + (dir + new Vec2(0f, -1f)).Normalized * 10f,
                    ServerData.Data.Weapons.Hammer.Damage, Player.ClientId, Weapon.HAMMER);
                hits++;
            }

            if (hits > 0)
                ReloadTimer = Server.TickSpeed / 3;
        }

        protected virtual void DoWeaponFire(Weapon weapon, Vec2 projStartPos, Vec2 direction)
        {
            switch (weapon)
            {
                case Weapon.HAMMER:
                    DoWeaponFireHammer(projStartPos, direction);
                    break;

                case Weapon.GUN:
                    DoWeaponFireGun(projStartPos, direction);
                    break;

                case Weapon.SHOTGUN:
                    DoWeaponFireShotgun(projStartPos, direction);
                    break;

                case Weapon.GRENADE:
                    DoWeaponFireGrenade(projStartPos, direction);
                    break;

                case Weapon.NINJA:
                    DoWeaponFireNinja(projStartPos, direction);
                    break;
            }
        }

        protected virtual void FireWeapon()
        {
            if (ReloadTimer != 0)
                return;

            DoWeaponSwitch();
             
            if (!CanFire()) 
                return;
            
            var direction = new Vec2(LatestInput.TargetX, LatestInput.TargetY).Normalized;
            var projStartPos = Position + direction * ProximityRadius * 0.75f;

            DoWeaponFire(ActiveWeapon, projStartPos, direction);

            AttackTick = Server.Tick;
            if (Weapons[(int) ActiveWeapon].Ammo > 0)
                Weapons[(int) ActiveWeapon].Ammo--;

            if (ReloadTimer == 0)
            {
                ReloadTimer = ServerData.Data.Weapons.Info[(int) ActiveWeapon]
                                  .FireDelay * Server.TickSpeed / 1000;
            }
        }

        public virtual bool TakeDamage(Vec2 force, int damage, int from, Weapon weapon)
        {
            Core.Velocity += force;

            if (GameContext.GameController.IsFriendlyFire(Player.ClientId, from) && !Config["SvTeamdamage"])
                return false;

            if (from == Player.ClientId)
                damage = System.Math.Max(1, damage / 2);

            DamageTaken++;

            if (Server.Tick < DamageTakenTick + 25)
            {
                GameContext.CreateDamageInd(Position, DamageTaken * 0.25f, damage);
            }
            else
            {
                DamageTaken = 0;
                GameContext.CreateDamageInd(Position, 0, damage);
            }

            if (damage != 0)
            {
                if (Armor != 0)
                {
                    if (damage > 1)
                    {
                        Health--;
                        damage--;
                    }

                    if (damage > Armor)
                    {
                        damage -= Armor;
                        Armor = 0;
                    }
                    else
                    {
                        Armor -= damage;
                        damage = 0;
                    }
                }

                Health -= damage;
            }

            DamageTakenTick = Server.Tick;

            if (from >= 0 && from != Player.ClientId && GameContext.Players[from] != null)
            {
                var mask = GameContext.MaskOne(from);
                for (var i = 0; i < GameContext.Players.Length; i++)
                {
                    if (GameContext.Players[i] == null)
                        continue;

                    if (GameContext.Players[i].Team == Team.SPECTATORS &&
                        GameContext.Players[i].SpectatorId == from)
                    {
                        mask |= GameContext.MaskOne(i);
                    }
                }
                GameContext.CreateSound(GameContext.Players[from].ViewPos, Sound.HIT, mask);
            }

            if (Health <= 0)
            {
                Die(from, weapon);

                if (from >= 0 && from != Player.ClientId && GameContext.Players[from] != null)
                {
                    var chr = GameContext.Players[from].GetCharacter();
                    chr?.SetEmote(Emote.HAPPY, Server.Tick + Server.TickSpeed);
                }

                return false;
            }

            GameContext.CreateSound(Position, damage > 2 
                ? Sound.PLAYER_PAIN_LONG 
                : Sound.PLAYER_PAIN_SHORT);

            SetEmote(Emote.PAIN, Server.Tick + 500 * Server.TickSpeed / 1000);
            return true;
        }

        protected virtual void HandleNinja()
        {
            if (ActiveWeapon != Weapon.NINJA)
                return;

            if (Server.Tick - NinjaStat.ActivationTick > 
                ServerData.Data.Weapons.Ninja.Duration * Server.Tick / 1000)
            {
                Weapons[(int) Weapon.NINJA].Got = false;
                ActiveWeapon = LastWeapon;

                SetWeapon(ActiveWeapon);
                return;
            }

            NinjaStat.CurrentMoveTime--;
            if (NinjaStat.CurrentMoveTime == 0)
            {
                Core.Velocity = NinjaStat.ActivationDir * NinjaStat.OldVelAmount;
            }

            if (NinjaStat.CurrentMoveTime <= 0)
                return;

            Core.Velocity = NinjaStat.ActivationDir * ServerData.Data.Weapons.Ninja.Velocity;
            var oldPos = Position;
            var vel = Core.Velocity;
            var pos = Core.Position;

            GameContext.Collision.MoveBox(ref pos, ref vel, new Vec2(ProximityRadius, ProximityRadius), 0f);
            Core.Velocity = Vec2.zero;
            Core.Position = pos;

            var dir = Position - oldPos;
            var radius = ProximityRadius * 2f;
            var center = oldPos + dir * 0.5f;

            foreach (var character in GameWorld.FindEntities<Character>(center, radius))
            {
                if (character == this)
                    continue;

                var alreadyHit = false;
                for (var i = 0; i < HitObjects.Count; i++)
                {
                    if (HitObjects[i] == character)
                        alreadyHit = true;
                }

                if (alreadyHit)
                    continue;
                
                if (Math.Distance(character.Position, Position) > ProximityRadius * 2f)
                    continue;

                GameContext.CreateSound(character.Position, Sound.NINJA_HIT);
                if (HitObjects.Count < 10)
                    HitObjects.Add(character);

                character.TakeDamage(new Vec2(0, -10.0f), ServerData.Data.Weapons.Ninja.Damage, 
                    Player.ClientId, Weapon.NINJA);
            }
        }

        protected virtual void HandleWeapons()
        {
            HandleNinja();

            if (ReloadTimer != 0)
            {
                ReloadTimer--;
                return;
            }

            FireWeapon();
            var ammoRegenTime = ServerData.Data.Weapons.Info[(int) ActiveWeapon].AmmoRegenTime;
            if (ammoRegenTime != 0)
            {
                if (ReloadTimer <= 0)
                {
                    if (Weapons[(int) ActiveWeapon].AmmoRegenStart < 0)
                        Weapons[(int) ActiveWeapon].AmmoRegenStart = Server.Tick;

                    if (Server.Tick - Weapons[(int) ActiveWeapon].AmmoRegenStart >=
                        ammoRegenTime * Server.TickSpeed / 1000)
                    {
                        Weapons[(int) ActiveWeapon].Ammo = System.Math.Clamp(
                            Weapons[(int) ActiveWeapon].Ammo + 1, 
                            1,
                            ServerData.Data.Weapons.Info[(int) ActiveWeapon].MaxAmmo
                        );
                        Weapons[(int) ActiveWeapon].AmmoRegenStart = -1;
                    }
                }
                else
                {
                    Weapons[(int) ActiveWeapon].AmmoRegenStart = -1;
                }
            }

        }

        public virtual void GiveNinja()
        {
            NinjaStat.ActivationTick = Server.Tick;
            Weapons[(int) Weapon.NINJA].Got = true;
            Weapons[(int) Weapon.NINJA].Ammo = -1;

            if (ActiveWeapon != Weapon.NINJA)
                LastWeapon = ActiveWeapon;

            ActiveWeapon = Weapon.NINJA;
            GameContext.CreateSound(Position, Sound.PICKUP_NINJA);
        }

        public virtual bool GiveWeapon(Weapon weapon, int ammo)
        {
            if (Weapons[(int) weapon].Ammo < ServerData.Data.Weapons.Info[(int) weapon].MaxAmmo ||
                !Weapons[(int) weapon].Got)
            {
                Weapons[(int) weapon].Got = true;
                Weapons[(int) weapon].Ammo = System.Math.Min(
                    ServerData.Data.Weapons.Info[(int) weapon].MaxAmmo, ammo);
                return true;
            }

            return false;
        }

        public virtual void ResetInput()
        {
            Input.Direction = 0;
            Input.Hook = false;

            if ((Input.Fire & 1) != 0)
                Input.Fire++;

            Input.Fire &= SnapObj_PlayerInput.INPUT_STATE_MASK;
            Input.Jump = false;
            
            LatestInput.FillFrom(Input);
            LatestPrevInput.FillFrom(Input);
        }

        public override void Tick()
        {
            Core.Input.FillFrom(Input);
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
            ReckoningCore.Tick(false);
            ReckoningCore.Move();
            ReckoningCore.Quantize();

            Core.Move();
            Core.Quantize();
            Position = Core.Position;

            var events = Core.TriggeredEvents;
            var mask = GameContext.MaskAllExceptOne(Player.ClientId);

            if (events.HasFlag(CoreEvents.GROUND_JUMP))
                GameContext.CreateSound(Position, Sound.PLAYER_JUMP, mask);

            if (events.HasFlag(CoreEvents.HOOK_ATTACH_PLAYER))
                GameContext.CreateSound(Position, Sound.HOOK_ATTACH_PLAYER, GameContext.MaskAll());

            if (events.HasFlag(CoreEvents.HOOK_ATTACH_GROUND))
                GameContext.CreateSound(Position, Sound.HOOK_ATTACH_GROUND, mask);

            if (events.HasFlag(CoreEvents.HOOK_HIT_NOHOOK))
                GameContext.CreateSound(Position, Sound.HOOK_NOATTACH, mask);

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
            DamageTakenTick++;

            if (LastAction != -1)
                LastAction++;

            if (EmoteStopTick > -1)
                EmoteStopTick++;

            if (Weapons[(int) ActiveWeapon].AmmoRegenStart > -1)
                Weapons[(int) ActiveWeapon].AmmoRegenStart++;
        }

        public virtual bool IncreaseHealth(int amount)
        {
            if (Health >= 10)
                return false;
            Health = System.Math.Clamp(Health + amount, 0, 10);
            return true;
        }

        public virtual bool IncreaseArmor(int amount)
        {
            if (Armor >= 10)
                return false;
            Armor = System.Math.Clamp(Armor + amount, 0, 10);
            return true;
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

                if (Weapons[(int) ActiveWeapon].Ammo > 0)
                    character.AmmoCount = Weapons[(int) ActiveWeapon].Ammo;
            }

            if (character.Emote == Emote.NORMAL)
            {
                if (250 - ((Server.Tick - LastAction) % 250) < 5)
                    character.Emote = Emote.BLINK;
            }


            if (character.HookedPlayer != -1)
            {
                if (!Server.Translate(ref character.HookedPlayer, snappingClient))
                    character.HookedPlayer = -1;
            }

            character.PlayerFlags = Player.PlayerFlags;
        }
    }
}
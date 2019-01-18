using System;
using System.Collections.Generic;
using TeeSharp.Common;
using TeeSharp.Common.Console;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Game;
using TeeSharp.Common.Protocol;

namespace TeeSharp.Server.Game.Entities
{
    public class Character : BaseCharacter
    {
        public override event CharacterDeadEvent Died;

        public override void Init(BasePlayer player, Vector2 spawnPos)
        {
            Player = player;
            Position = spawnPos;

            IsAlive = true;
            Health = 0;
            Armor = 0;
            EmoteStopTick = -1;
            Emote = Emote.Normal;

            Core = Kernel.Get<BaseCharacterCore>();
            SendCore = Kernel.Get<BaseCharacterCore>();
            ReckoningCore = Kernel.Get<BaseCharacterCore>();
            ReckoningCore.IsPredicted = true;

            Core.Reset();
            Core.Init(GameWorld.WorldCore, GameContext.MapCollision);
            Core.Position = Position;

            var worldCore = new WorldCore(
                GameWorld.WorldCore.CharacterCores.Length,
                Kernel.Get<BaseTuningParams>());
            ReckoningCore.Init(worldCore, GameContext.MapCollision);

            GameContext.CreatePlayerSpawn(spawnPos);
            GameWorld.WorldCore.CharacterCores[player.ClientId] = Core;

            Input = new SnapshotPlayerInput();
            LatestPrevInput = new SnapshotPlayerInput();
            LatestInput = new SnapshotPlayerInput();
            LastAction = -1;

            HitObjects = new List<Entity>();
            NinjaStat = new NinjaStat();

            ActiveWeapon = Weapon.Hammer;
            LastWeapon = Weapon.Gun;
            QueuedWeapon = (Weapon) (-1);
            Weapons = new WeaponStat[(int) Weapon.NumWeapons];
            Weapons[(int) Weapon.Hammer].Got = true;
            Weapons[(int) Weapon.Hammer].Ammo = -1;

            Destroyed += OnDestroyed;
            Reseted += OnReseted;
        }

        protected override void OnReseted(Entity character)
        {
            Destroy();
        }

        protected void OnDestroyed(Entity character)
        {
            GameWorld.WorldCore.CharacterCores[Player.ClientId] = null;
            IsAlive = false;
        }

        public override void SetWeapon(Weapon weapon)
        {
            if (ActiveWeapon == weapon || ActiveWeapon == Weapon.Ninja)
                return;

            LastWeapon = ActiveWeapon;
            QueuedWeapon = (Weapon)(-1);
            ActiveWeapon = weapon;
            GameContext.CreateSound(Position, Sound.WeaponSwitch);

            if (ActiveWeapon < 0 || ActiveWeapon >= Weapon.NumWeapons)
                ActiveWeapon = Weapon.Hammer;

            Weapons[(int) ActiveWeapon].AmmoRegenStart = 0;
        }
        
        public override void SetEmote(Emote emote, int stopTick)
        {
            Emote = emote;
            EmoteStopTick = stopTick;
        }

        public override void Die(int killer, Weapon weapon)
        {
            IsAlive = false;
            Player.RespawnTick = Server.Tick + Server.TickSpeed / 2;
            var modeSpecial = 0;
            Died?.Invoke(this, GameContext.Players[killer], weapon, ref modeSpecial);

            Console.Print(OutputLevel.Debug, "game",
                $"kill killer='{killer}:{Server.ClientName(killer)}' " +
                $"victim='{Player.ClientId}:{Server.ClientName(Player.ClientId)}' " +
                $"weapon={weapon} special={modeSpecial}");

            Server.SendPackMsg(new GameMsg_SvKillMsg
            {
                Killer = killer,
                Victim = Player.ClientId,
                Weapon = (int) weapon,
                ModeSpecial = modeSpecial
            }, MsgFlags.Vital, -1);

            GameContext.CreateSound(Position, Sound.PlayerDie);
            GameContext.CreateDeath(Position, Player.ClientId);

            Destroy();
        }

        public override void OnPredictedInput(SnapshotPlayerInput newInput)
        {
            if (!Input.Equals(newInput))
                LastAction = Server.Tick;

            Input.Fill(newInput);
            NumInputs++;

            if (Input.TargetX == 0 && Input.TargetY == 0)
                Input.TargetY = -1;
        }

        public override void OnDirectInput(SnapshotPlayerInput newInput)
        {
            LatestPrevInput.Fill(LatestInput);
            LatestInput.Fill(newInput);

            if (LatestInput.TargetX == 0 && LatestInput.TargetY == 0)
                LatestInput.TargetY = -1;

            if (NumInputs > 2 && Player.Team != Team.Spectators)
            {
                HandleWeaponSwitch();
                FireWeapon();
            }

            LatestPrevInput.Fill(LatestInput);
        }

        protected override void DoWeaponSwitch()
        {
            if (ReloadTimer != 0 || QueuedWeapon == (Weapon) (-1) || Weapons[(int) Weapon.Ninja].Got)
                return;

            SetWeapon(QueuedWeapon);
        }

        protected override void HandleWeaponSwitch()
        {
            var wantedWeapon = ActiveWeapon;
            if (QueuedWeapon != (Weapon) (-1))
                wantedWeapon = QueuedWeapon;

            var next = InputCount.Count((int) LatestPrevInput.NextWeapon,
                (int) LatestInput.NextWeapon).Presses;
            var prev = InputCount.Count((int) LatestPrevInput.PreviousWeapon,
                (int) LatestInput.PreviousWeapon).Presses;

            if (next < 128)
            {
                while (next != 0)
                {
                    wantedWeapon = (Weapon) ((int) (wantedWeapon + 1) % (int) Weapon.NumWeapons);
                    if (Weapons[(int) wantedWeapon].Got)
                        next--;
                }
            }

            if (prev < 128)
            {
                while (prev != 0)
                {
                    wantedWeapon = wantedWeapon - 1 < 0
                        ? Weapon.NumWeapons - 1
                        : wantedWeapon - 1;
                    if (Weapons[(int) wantedWeapon].Got)
                        prev--;
                }
            }

            if (LatestInput.WantedWeapon != 0)
                wantedWeapon = (Weapon) (Input.WantedWeapon - 1);

            if (wantedWeapon >= 0 &&
                wantedWeapon < Weapon.NumWeapons &&
                wantedWeapon != ActiveWeapon &&
                Weapons[(int) wantedWeapon].Got)
            {
                QueuedWeapon = wantedWeapon;
            }

            DoWeaponSwitch();
        }

        protected override bool CanFire()
        {
            var fullAuto = ActiveWeapon == Weapon.Grenade ||
                           ActiveWeapon == Weapon.Shotgun ||
                           ActiveWeapon == Weapon.Laser;

            var willFire = InputCount.Count(LatestPrevInput.Fire, LatestInput.Fire).Presses != 0 ||
                           fullAuto && (LatestInput.Fire & 1) != 0 && Weapons[(int)ActiveWeapon].Ammo != 0;

            if (!willFire)
                return false;

            if (Weapons[(int)ActiveWeapon].Ammo == 0)
            {
                ReloadTimer = 125 * Server.TickSpeed / 1000;
                if (LastNoAmmoSound + Server.TickSpeed <= Server.Tick)
                {
                    GameContext.CreateSound(Position, Sound.WeaponNoAmmo);
                    LastNoAmmoSound = Server.Tick;
                }
                return false;
            }

            return true;
        }

        protected override void DoWeaponFireRifle(Vector2 startPos, Vector2 direction)
        {
            var laser = new Laser(Position, direction, Tuning["laser_reach"], Player.ClientId);
            GameContext.CreateSound(Position, Sound.LaserFire);
        }

        protected override void DoWeaponFireNinja(Vector2 startPos, Vector2 direction)
        {
            HitObjects.Clear();
            NinjaStat.ActivationDirection = direction;
            NinjaStat.CurrentMoveTime = ServerData.Weapons.Ninja.MoveTime * Server.TickSpeed / 1000;
            NinjaStat.OldVelAmount = Core.Velocity.Length;

            GameContext.CreateSound(Position, Sound.NinjaFire);
        }

        protected override void DoWeaponFireGrenade(Vector2 startPos, Vector2 direction)
        {
            var projectile = new Projectile
            (
                weapon: Weapon.Grenade,
                ownerId: Player.ClientId,
                startPos: startPos,
                direction: direction,
                lifeSpan: (int) (Server.TickSpeed * Tuning["grenade_lifetime"]),
                damage: ServerData.Weapons.Grenade.Damage,
                explosive: true,
                force: 0f,
                soundImpact: Sound.GrenadeExplode
            );

            GameContext.CreateSound(Position, Sound.GrenadeFire);
        }

        protected override void DoWeaponFireShotgun(Vector2 startPos, Vector2 direction)
        {
            const int ShotSpread = 2;
            var spreading = new[] {-0.185f, -0.070f, 0f, 0.070f, 0.185f};

            for (var i = -ShotSpread; i <= ShotSpread; i++)
            {
                var angle = MathHelper.Angle(direction);
                angle += spreading[i + ShotSpread];
                var v = 1 - (Math.Abs(i) / (float) ShotSpread);
                var speed = MathHelper.Mix(Tuning["shotgun_speeddiff"], 1.0f, v);
                var projectile = new Projectile
                (
                    weapon: Weapon.Shotgun,
                    ownerId: Player.ClientId,
                    startPos: startPos,
                    direction: new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed,
                    lifeSpan: (int) (Server.TickSpeed * Tuning["shotgun_lifetime"]),
                    damage: ServerData.Weapons.Shotgun.Damage,
                    explosive: false,
                    force: 0f,
                    soundImpact: (Sound) (-1)
                );
            }

            GameContext.CreateSound(Position, Sound.ShotgunFire);
        }

        protected override void DoWeaponFireGun(Vector2 startPos, Vector2 direction)
        {
            var projectile = new Projectile
            (
                weapon: Weapon.Gun,
                ownerId: Player.ClientId,
                startPos: startPos,
                direction: direction,
                lifeSpan: (int) (Server.TickSpeed * Tuning[key: "gun_lifetime"]),
                damage: 1,
                explosive: false,
                force: 0f,
                soundImpact: (Sound) (-1)
            );

            GameContext.CreateSound(Position, Sound.GunFire);
        }

        protected override void DoWeaponFireHammer(Vector2 startPos, Vector2 direction)
        {
            GameContext.CreateSound(Position, Sound.HammerFire);

            foreach (var target in Entities.Find(startPos, ProximityRadius * 0.5f))
            {
                if (target == this)
                    continue;

                var collision = GameContext.MapCollision.IntersectLine(startPos, target.Position, out _, out _);
                if (collision.HasFlag(CollisionFlags.Solid))
                    continue;

                if ((target.Position - startPos).Length > 0f)
                {
                    GameContext.CreateHammerHit(target.Position - 
                                               (target.Position - startPos).Normalized * ProximityRadius * 0.5f);
                }
                else
                    GameContext.CreateHammerHit(startPos);

                var forceDirection = (target.Position - Position).Length > 0f 
                                   ? (target.Position - Position).Normalized 
                                   : new Vector2(0f, -1f);

                target.TakeDamage(
                    force: new Vector2(0, -1f) + (forceDirection + new Vector2(0f, -1.1f)).Normalized * 10f,
                    source: forceDirection * -1,
                    damage: ServerData.Weapons.Hammer.Damage,
                    from: Player.ClientId,
                    weapon: Weapon.Hammer);

                ReloadTimer = Server.TickSpeed / 3;
            }
        }

        protected override void DoWeaponFire(Weapon weapon, Vector2 startPos, Vector2 direction)
        {
            switch (weapon)
            {
                case Weapon.Hammer:
                    DoWeaponFireHammer(startPos, direction);
                    break;

                case Weapon.Gun:
                    DoWeaponFireGun(startPos, direction);
                    break;

                case Weapon.Shotgun:
                    DoWeaponFireShotgun(startPos, direction);
                    break;

                case Weapon.Grenade:
                    DoWeaponFireGrenade(startPos, direction);
                    break;

                case Weapon.Ninja:
                    DoWeaponFireNinja(startPos, direction);
                    break;

                case Weapon.Laser:
                    DoWeaponFireRifle(startPos, direction);
                    break;
            }
        }

        protected override void FireWeapon()
        {
            if (ReloadTimer != 0)
                return;

            DoWeaponSwitch();

            if (!CanFire())
                return;

            var direction = new Vector2(LatestInput.TargetX, LatestInput.TargetY).Normalized;
            var projStartPos = Position + direction * ProximityRadius * 0.75f;

            DoWeaponFire(ActiveWeapon, projStartPos, direction);

            AttackTick = Server.Tick;
            if (Weapons[(int)ActiveWeapon].Ammo > 0)
                Weapons[(int)ActiveWeapon].Ammo--;

            if (ReloadTimer == 0)
                ReloadTimer = ServerData.Weapons[ActiveWeapon].FireDelay * Server.TickSpeed / 1000;
        }

        public override bool TakeDamage(Vector2 force, Vector2 source, int damage, int from, Weapon weapon)
        {
            Core.Velocity += force;

            if (GameContext.GameController.IsFriendlyFire(Player.ClientId, from))
                return false;

            if (from == Player.ClientId)
                damage = Math.Max(1, damage / 2);

            var prevHealth = Health;
            var prevArmor = Armor;

            if (damage > 0)
            {
                if (Armor > 0)
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

            GameContext.CreateDamageIndicator(Position, source, Player.ClientId, 
                prevHealth - Health, 
                prevArmor - Armor, from == Player.ClientId);

            bool AttackerExist() => from >= 0 && from != Player.ClientId && GameContext.Players[from] != null;

            if (AttackerExist())
            {
                var mask = BaseGameContext.MaskOne(from);
                for (var i = 0; i < GameContext.Players.Length; i++)
                {
                    if (GameContext.Players[i] != null && (
                            GameContext.Players[i].Team == Team.Spectators ||
                            GameContext.Players[i].DeadSpectatorMode) &&
                        GameContext.Players[i].SpectatorId == from)
                    {
                        mask |= BaseGameContext.MaskOne(i);
                    }
                }

                GameContext.CreateSound(GameContext.Players[from].ViewPos, Sound.Hit, mask);
            }

            if (Health <= 0)
            {
                Die(from, weapon);

                if (AttackerExist())
                {
                    GameContext.Players[from]
                        .GetCharacter()?
                        .SetEmote(Emote.Happy, Server.Tick + Server.TickSpeed);
                }

                return false;
            }

            GameContext.CreateSound(Position, damage > 2 
                ? Sound.PlayerPainLong 
                : Sound.PlayerPainShort);

            SetEmote(Emote.Pain, Server.Tick + Server.TickSpeed / 2);
            return true;
        }

        protected override void HandleNinja()
        {
            if (ActiveWeapon != Weapon.Ninja)
                return;

            void ResetVelocity()
            {
                Core.Velocity = NinjaStat.ActivationDirection * NinjaStat.OldVelAmount;
            }

            if (Server.Tick - NinjaStat.ActivationTick > ServerData.Weapons.Ninja.Duration * Server.TickSpeed / 1000)
            {
                Weapons[(int)Weapon.Ninja].Got = false;
                ActiveWeapon = LastWeapon;

                if (NinjaStat.CurrentMoveTime > 0)
                    ResetVelocity();
                return;
            }

            NinjaStat.CurrentMoveTime--;
            if (NinjaStat.CurrentMoveTime == 0)
                ResetVelocity();

            if (NinjaStat.CurrentMoveTime > 0)
            {
                Core.Velocity = NinjaStat.ActivationDirection * ServerData.Weapons.Ninja.Velocity;
                var previousPos = Core.Position;
                var pos = Core.Position;
                var velocity = Core.Velocity;
                GameContext.MapCollision.MoveBox(ref pos, ref velocity, new Vector2(ProximityRadius, ProximityRadius), 0);

                Core.Position = pos;
                Core.Velocity = Vector2.Zero;

                var direction = previousPos - pos;
                var radius = ProximityRadius * 2f;
                var center = previousPos + direction * 0.5f;

                foreach (var hitCharacter in Entities.Find(center, radius))
                {
                    if (hitCharacter == this)
                        continue;
                    
                    if (HitObjects.Contains(hitCharacter))
                        continue;

                    if (MathHelper.Distance(hitCharacter.Position, pos) > ProximityRadius * 2f)
                        continue;

                    GameContext.CreateSound(pos, Sound.NinjaHit);
                    HitObjects.Add(hitCharacter);
                    hitCharacter.TakeDamage(
                        force: new Vector2(0, -10f), 
                        source: NinjaStat.ActivationDirection * -1,
                        damage: ServerData.Weapons.Ninja.Damage,
                        from: Player.ClientId, 
                        weapon: Weapon.Ninja);

                }
            }
        }

        protected override void HandleWeapons()
        {
            HandleNinja();

            if (ReloadTimer != 0)
            {
                ReloadTimer--;
                return;
            }

            FireWeapon();

            var ammoRegenTime = ServerData.Weapons[ActiveWeapon].AmmoRegenTime;
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
                            ServerData.Weapons[ActiveWeapon].MaxAmmo
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

        public override void GiveNinja()
        {
            NinjaStat.ActivationTick = Server.Tick;
            NinjaStat.CurrentMoveTime = -1;

            Weapons[(int) Weapon.Ninja].Got = true;
            Weapons[(int) Weapon.Ninja].Ammo = -1;

            if (ActiveWeapon != Weapon.Ninja)
            {
                LastWeapon = ActiveWeapon;
                ActiveWeapon = Weapon.Ninja;
            }

            GameContext.CreateSound(Position, Sound.PickupNinja);

        }

        public override bool GiveWeapon(Weapon weapon, int ammo)
        {
            if (!Weapons[(int) weapon].Got || Weapons[(int) weapon].Ammo < ServerData.Weapons[weapon].MaxAmmo)
            {
                Weapons[(int) weapon].Got = true;
                Weapons[(int) weapon].Ammo = Math.Min(ServerData.Weapons[weapon].MaxAmmo, ammo);
                return true;
            }

            return false;
        }

        public override void ResetInput()
        {
            Input.Direction = 0;
            Input.IsHook = false;

            if ((Input.Fire & 1) != 0)
                Input.Fire++;

            Input.Fire &= SnapshotPlayerInput.StateMask;
            Input.IsJump = false;
            
            LatestInput.Fill(Input);
            LatestPrevInput.Fill(Input);
        }

        public override void Tick()
        {
            Core.Tick(Input);

            var rDiv3 = ProximityRadius / 3.0f;

            if (GameContext.MapCollision.GetTileFlags(Position.x + rDiv3, Position.y - rDiv3).HasFlag(CollisionFlags.Death) ||
                GameContext.MapCollision.GetTileFlags(Position.x + rDiv3, Position.y + rDiv3).HasFlag(CollisionFlags.Death) ||
                GameContext.MapCollision.GetTileFlags(Position.x - rDiv3, Position.y - rDiv3).HasFlag(CollisionFlags.Death) ||
                GameContext.MapCollision.GetTileFlags(Position.x - rDiv3, Position.y + rDiv3).HasFlag(CollisionFlags.Death) ||
                GameLayerClipped(Position))
            {
                Die(Player.ClientId, BasePlayer.WeaponWorld);
            }

            HandleWeapons();
        }

        public override void LateTick()
        {
            ReckoningCore.Tick(null);
            ReckoningCore.Move();
            ReckoningCore.Quantize();

            Core.Move();
            Core.Quantize();
            Position = Core.Position;

            if (Player.Team == Team.Spectators)
                Position = new Vector2(Input.TargetX, Input.TargetY);

            {
                var predicted = new SnapshotCharacter();
                var current = new SnapshotCharacter();

                ReckoningCore.Write(predicted);
                Core.Write(current);

                if (ReckoningTick + Server.TickSpeed * 3 < Server.Tick || !current.Equals(predicted))
                {
                    ReckoningTick = Server.Tick;
                    SendCore.Fill(Core);
                    ReckoningCore.Fill(Core);
                }
            }
        }

        public override void TickPaused()
        {
            AttackTick++;
            ReckoningTick++;
            NinjaStat.ActivationTick++;

            if (LastAction != -1)
                LastAction++;

            if (EmoteStopTick > -1)
                EmoteStopTick++;

            if (Weapons[(int) ActiveWeapon].AmmoRegenStart > -1)
                Weapons[(int) ActiveWeapon].AmmoRegenStart++;
        }

        public override bool IncreaseHealth(int amount)
        {
            if (Health >= 10)
                return false;
            Health = Math.Clamp(Health + amount, 0, 10);
            return true;
        }

        public override bool IncreaseArmor(int amount)
        {
            if (Armor >= 10)
                return false;
            Armor = Math.Clamp(Armor + amount, 0, 10);
            return true;
        }

        public override void OnSnapshot(int snappingClient)
        {
            if (NetworkClipped(snappingClient))
                return;

            var character = Server.SnapshotItem<SnapshotCharacter>(Player.ClientId);
            if (character == null)
                return;

            if (ReckoningTick == 0 || GameWorld.Paused)
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
                Emote = Emote.Normal;
            }

            character.Emote = Emote;
            character.AmmoCount = 0;
            character.Health = 0;
            character.Armor = 0;
            character.TriggeredEvents = Core.TriggeredEvents;

            character.Weapon = ActiveWeapon;
            character.AttackTick = AttackTick;
            character.Direction = Input.Direction;

            if (snappingClient == Player.ClientId || 
                snappingClient == -1 || !Config["SvStrictSpectateMode"] && 
                Player.ClientId == GameContext.Players[snappingClient].SpectatorId)
            {
                character.Health = Health;
                character.Armor = Armor;

                if (ActiveWeapon == Weapon.Ninja)
                {
                    character.AmmoCount = NinjaStat.ActivationTick +
                                          ServerData.Weapons.Ninja.Duration * Server.TickSpeed / 1000;
                }
                else if (Weapons[(int) ActiveWeapon].Ammo > 0)
                    character.AmmoCount = Weapons[(int) ActiveWeapon].Ammo;
            }

            if (character.Emote == Emote.Normal)
            {
                if (250 - ((Server.Tick - LastAction) % 250) < 5)
                    character.Emote = Emote.Blink;
            }
        }
    }
}
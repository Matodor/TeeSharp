using System.Collections.Generic;
using TeeSharp.Common;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Protocol;

namespace TeeSharp.Server.Game.Entities
{
    public struct WeaponStat
    {
        public int AmmoRegenStart { get; set; }
        public int Ammo { get; set; }
        public int AmmoCost { get; set; }
        public bool Got { get; set; }
    }

    public class NinjaStat
    {
        public Vector2 ActivationDirection;
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
            var c = new InputCount() { Presses = 0, Releases = 0 };
            prev &= SnapshotPlayerInput.StateMask;
            cur &= SnapshotPlayerInput.StateMask;
            var i = prev;

            while (i != cur)
            {
                i = (i + 1) & SnapshotPlayerInput.StateMask;
                if ((i & 1) != 0)
                    c.Presses++;
                else
                    c.Releases++;
            }

            return c;
        }
    }

    public delegate void CharacterDeadEvent(BaseCharacter victim,
        BasePlayer killer, Weapon weapon, ref int modeSpecial);

    public abstract class BaseCharacter : Entity<BaseCharacter>
    {
        public abstract event CharacterDeadEvent Died;

        public override float ProximityRadius { get; protected set; } = 28f;

        public virtual BasePlayer Player { get; protected set; }
        public virtual bool IsAlive { get; protected set; }
        public virtual int Health { get; protected set; }
        public virtual int Armor { get; protected set; }

        protected virtual BaseCharacterCore Core { get; set; }
        protected virtual BaseCharacterCore SendCore { get; set; }
        protected virtual BaseCharacterCore ReckoningCore { get; set; }

        protected virtual int ReckoningTick { get; set; }
        protected virtual int EmoteStopTick { get; set; }
        protected virtual Emote Emote { get; set; }

        protected virtual SnapshotPlayerInput Input { get; set; }
        protected virtual SnapshotPlayerInput LatestPrevInput { get; set; }
        protected virtual SnapshotPlayerInput LatestInput { get; set; }
        protected virtual int LastAction { get; set; }
        protected virtual int NumInputs { get; set; }

        protected virtual Weapon ActiveWeapon { get; set; }
        protected virtual Weapon LastWeapon { get; set; }
        protected virtual Weapon QueuedWeapon { get; set; }
        protected virtual WeaponStat[] Weapons { get; set; }

        protected virtual int AttackTick { get; set; }
        protected virtual int ReloadTimer { get; set; }
        protected virtual int LastNoAmmoSound { get; set; }
        protected virtual NinjaStat NinjaStat { get; set; }
        protected virtual IList<Entity> HitObjects { get; set; }

        public abstract void Init(BasePlayer player, Vector2 spawnPos);

        public BaseCharacter() : base(1)
        {
        }

        public abstract void OnDirectInput(SnapshotPlayerInput newInput);
        public abstract void OnPredictedInput(SnapshotPlayerInput newInput);
        public abstract void Die(int killer, Weapon weapon);
        public abstract void SetEmote(Emote emote, int stopTick);
        public abstract void SetWeapon(Weapon weapon);

        public abstract bool IncreaseArmor(int amount);
        public abstract bool IncreaseHealth(int amount);
        public abstract void ResetInput();
        public abstract bool GiveWeapon(Weapon weapon, int ammo);
        public abstract void GiveNinja();
        public abstract bool TakeDamage(Vector2 force, Vector2 source, int damage, int @from, Weapon weapon);

        protected abstract void HandleWeapons();
        protected abstract void HandleNinja(); protected abstract void FireWeapon();
        protected abstract void DoWeaponFire(Weapon weapon, Vector2 startPos, Vector2 direction);
        protected abstract void DoWeaponFireHammer(Vector2 startPos, Vector2 direction);
        protected abstract void DoWeaponFireGun(Vector2 startPos, Vector2 direction);
        protected abstract void DoWeaponFireShotgun(Vector2 startPos, Vector2 direction);
        protected abstract void DoWeaponFireGrenade(Vector2 startPos, Vector2 direction);
        protected abstract void DoWeaponFireNinja(Vector2 startPos, Vector2 direction);
        protected abstract void DoWeaponFireRifle(Vector2 startPos, Vector2 direction);
        protected abstract bool CanFire();
        protected abstract void HandleWeaponSwitch();
        protected abstract void DoWeaponSwitch();
        protected abstract void OnReseted(Entity character);
    }
}
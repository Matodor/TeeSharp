using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Protocol
{
    public class SnapObj_Character : BaseSnapObject
    {
        public override SnapshotItem Type { get; } = SnapshotItem.OBJ_CHARACTER;
        public override int SerializeLength { get; } = 22;

        public int Tick;
        public int PosX;
        public int PosY;
        public int VelX;
        public int VelY;
        public int Angle;
        public int Direction;
        public int Jumped;
        public int HookedPlayer;
        public HookState HookState = HookState.IDLE;
        public int HookTick;
        public int HookX;
        public int HookY;
        public int HookDx;
        public int HookDy;
        public PlayerFlags PlayerFlags = PlayerFlags.NONE;
        public int Health;
        public int Armor;
        public int AmmoCount;
        public Weapon Weapon = Weapon.HAMMER;
        public Emote Emote = Emote.NORMAL;
        public int AttackTick;

        public bool Compare(SnapObj_Character other)
        {
            return
                Tick == other.Tick &&
                PosX == other.PosX &&
                PosY == other.PosY &&
                VelX == other.VelX &&
                VelY == other.VelY &&
                Angle == other.Angle &&
                Direction == other.Direction &&
                Jumped == other.Jumped &&
                HookedPlayer == other.HookedPlayer &&
                HookState == other.HookState &&
                HookTick == other.HookTick &&
                HookX == other.HookX &&
                HookY == other.HookY &&
                HookDx == other.HookDx &&
                HookDy == other.HookDy &&

                PlayerFlags == other.PlayerFlags &&
                Health == other.Health &&
                Armor == other.Armor &&
                AmmoCount == other.AmmoCount &&
                Weapon == other.Weapon &&
                Emote == other.Emote &&
                AttackTick == other.AttackTick;
        }

        public override int[] Serialize()
        {
            return new[]
            {
                Tick,
                PosX,
                PosY,
                VelX,
                VelY,
                Angle,
                Direction,
                Jumped,
                HookedPlayer,
                (int) HookState,
                HookTick,
                HookX,
                HookY,
                HookDx,
                HookDy,

                (int) PlayerFlags,
                Health,
                Armor,
                AmmoCount,
                (int) Weapon,
                (int) Emote,
                AttackTick,
            };
        }
    }
}
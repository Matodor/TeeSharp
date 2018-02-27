using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    public class SnapObj_Character : BaseSnapObject
    {
        public override SnapObject Type { get; } = SnapObject.OBJ_CHARACTER;
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

        public override void Deserialize(int[] data, int dataOffset)
        {
            if (!RangeCheck(data, dataOffset))
                return;

            Tick = data[dataOffset + 0];
            PosX = data[dataOffset + 1];
            PosY = data[dataOffset + 2];
            VelX = data[dataOffset + 3];
            VelY = data[dataOffset + 4];

            Angle = data[dataOffset + 5];
            Direction = data[dataOffset + 6];
            Jumped = data[dataOffset + 7];
            HookedPlayer = data[dataOffset + 8];
            HookState = (HookState) data[dataOffset + 9];
            HookTick = data[dataOffset + 10];
            HookX = data[dataOffset + 11];
            HookY = data[dataOffset + 12];
            HookDx = data[dataOffset + 13];
            HookDy = data[dataOffset + 14];

            PlayerFlags = (PlayerFlags) data[dataOffset + 15];
            Health = data[dataOffset + 16];
            Armor = data[dataOffset + 17];
            AmmoCount = data[dataOffset + 18];
            Weapon = (Weapon) data[dataOffset + 19];
            Emote = (Emote) data[dataOffset + 20];
            AttackTick = data[dataOffset + 21];
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

        public override string ToString()
        {
            return $"SnapObj_Character tick={Tick} pos={PosX}:{PosY} vel={VelX}:{VelY}" +
                   $" angle={Angle} dir={Direction} jumped={Jumped} hookedPlayer={HookedPlayer}" +
                   $" hookState={HookState} hootTick={HookTick} hook={HookX}:{HookY}" +
                   $" hookDelta={HookDx}:{HookDy} playerFlags={PlayerFlags} health={Health}" +
                   $" armor={Armor} ammoCount={AmmoCount} weapon={Weapon} emote={Emote} attackTick={AttackTick}";
        }
    }
}
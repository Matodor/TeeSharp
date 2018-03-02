using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    public class SnapObj_PlayerInput : BaseSnapObject
    {
        public const int INPUT_STATE_MASK = 0b11_1111;

        public override SnapObject Type { get; } = SnapObject.OBJ_PLAYERINPUT;
        public override int SerializeLength { get; } = 10;

        public int Direction;
        public int TargetX;
        public int TargetY;
        public bool Jump;
        public int Fire;
        public bool Hook;
        public PlayerFlags PlayerFlags = 0;
        public int WantedWeapon;
        public int NextWeapon;
        public int PrevWeapon;

        public void Reset()
        {
            Direction = 0;
            TargetX = 0;
            TargetY = 0;
            Jump = false;
            Fire = 0;
            Hook = false;
            PlayerFlags = 0;
            WantedWeapon = 0;
            NextWeapon = 0;
            PrevWeapon = 0;
    }

        public bool Compare(SnapObj_PlayerInput other)
        {
            return
                Direction == other.Direction &&
                TargetX == other.TargetX &&
                TargetY == other.TargetY &&
                Jump == other.Jump &&
                Fire == other.Fire &&
                Hook == other.Hook &&
                PlayerFlags == other.PlayerFlags &&
                WantedWeapon == other.WantedWeapon &&
                NextWeapon == other.NextWeapon &&
                PrevWeapon == other.PrevWeapon;
        }

        public void FillFrom(SnapObj_PlayerInput other)
        {
            Direction = other.Direction;
            TargetX = other.TargetX;
            TargetY = other.TargetY;
            Jump = other.Jump;
            Fire = other.Fire;
            Hook = other.Hook;
            PlayerFlags = other.PlayerFlags;
            WantedWeapon = other.WantedWeapon;
            NextWeapon = other.NextWeapon;
            PrevWeapon = other.PrevWeapon;
        }

        public override void Deserialize(int[] data, int dataOffset)
        {
            if (!RangeCheck(data, dataOffset))
                return;

            Direction = data[dataOffset + 0];
            TargetX = data[dataOffset + 1];
            TargetY = data[dataOffset + 2];
            Jump = data[dataOffset + 3] != 0;
            Fire = data[dataOffset + 4];
            Hook = data[dataOffset + 5] != 0;
            PlayerFlags = (PlayerFlags) data[dataOffset + 6];
            WantedWeapon = data[dataOffset + 7];
            NextWeapon = data[dataOffset + 8];
            PrevWeapon = data[dataOffset + 9];
        }
        
        public override int[] Serialize()
        {
            return new[]
            {
                Direction,
                TargetX,
                TargetY,
                Jump ? 1 : 0,
                Fire,
                Hook ? 1 : 0,
                (int) PlayerFlags,
                WantedWeapon,
                NextWeapon,
                PrevWeapon,
            };
        }

        public override string ToString()
        {
            return $"SnapObj_PlayerInput dir={Direction} target={TargetX}:{TargetY}" +
                   $" jump={Jump} fire={Fire} hook={Hook} playerFlags={PlayerFlags}" +
                   $" wantedWeapon={WantedWeapon} nextWeapon={NextWeapon} prevWeapon={PrevWeapon}";
        }
    }
}
using System;
using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class SnapshotPlayerInput : BaseSnapshotItem, IEquatable<SnapshotPlayerInput>
    {
        public const int StateMask = 0b111111;

        public override SnapshotItems Type => SnapshotItems.PlayerInput;

        public bool IsJump
        {
            get => Jump != 0;
            set => Jump = value ? 1 : 0;
        }

        public bool IsHook
        {
            get => Hook != 0;
            set => Hook = value ? 1 : 0;
        }

        [MarshalAs(UnmanagedType.I4)] public int Direction;
        [MarshalAs(UnmanagedType.I4)] public int TargetX;
        [MarshalAs(UnmanagedType.I4)] public int TargetY;
        [MarshalAs(UnmanagedType.I4)] public int Jump;
        [MarshalAs(UnmanagedType.I4)] public int Fire;
        [MarshalAs(UnmanagedType.I4)] public int Hook;
        [MarshalAs(UnmanagedType.I4)] public PlayerFlags PlayerFlags;
        [MarshalAs(UnmanagedType.I4)] public Weapon WantedWeapon;
        [MarshalAs(UnmanagedType.I4)] public Weapon NextWeapon;
        [MarshalAs(UnmanagedType.I4)] public Weapon PreviousWeapon;

        public bool IsValid()
        {
            if (Direction < -1 || Direction > 1)
                return false;
            if (Jump < 0 || Jump > 1)
                return false;
            if (Hook < 0 || Hook > 1)
                return false;
            if ((PlayerFlags & PlayerFlags.All) != PlayerFlags)
                return false;
            if (WantedWeapon < 0 || WantedWeapon >= Weapon.NumWeapons)
                return false;
            return true;
        }

        public void Fill(SnapshotPlayerInput from)
        {
            Direction = from.Direction;
            TargetX = from.TargetX;
            TargetY = from.TargetY;
            Jump = from.Jump;
            Fire = from.Fire;
            Hook = from.Hook;
            PlayerFlags = from.PlayerFlags;
            WantedWeapon = from.WantedWeapon;
            NextWeapon = from.NextWeapon;
            PreviousWeapon = from.PreviousWeapon;
        }

        public bool Equals(SnapshotPlayerInput other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Direction == other.Direction && 
                   TargetX == other.TargetX && 
                   TargetY == other.TargetY &&
                   Jump == other.Jump && 
                   Fire == other.Fire && 
                   Hook == other.Hook &&
                   PlayerFlags == other.PlayerFlags &&
                   WantedWeapon == other.WantedWeapon && 
                   NextWeapon == other.NextWeapon &&
                   PreviousWeapon == other.PreviousWeapon;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SnapshotPlayerInput) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Direction;
                hashCode = (hashCode * 397) ^ TargetX;
                hashCode = (hashCode * 397) ^ TargetY;
                hashCode = (hashCode * 397) ^ Jump;
                hashCode = (hashCode * 397) ^ Fire;
                hashCode = (hashCode * 397) ^ Hook;
                hashCode = (hashCode * 397) ^ (int) PlayerFlags;
                hashCode = (hashCode * 397) ^ (int) WantedWeapon;
                hashCode = (hashCode * 397) ^ (int) NextWeapon;
                hashCode = (hashCode * 397) ^ (int) PreviousWeapon;
                return hashCode;
            }
        }
    }
}
using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class SnapshotPlayerInput : BaseSnapshotItem
    {
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
    }
}
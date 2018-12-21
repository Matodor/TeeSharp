using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class SnapObj_Projectile : BaseSnapObject
    {
        public override SnapshotItems Type => SnapshotItems.Projectile;

        [MarshalAs(UnmanagedType.I4)] public int X;
        [MarshalAs(UnmanagedType.I4)] public int Y;
        [MarshalAs(UnmanagedType.I4)] public int VelX;
        [MarshalAs(UnmanagedType.I4)] public int VelY;
        [MarshalAs(UnmanagedType.I4)] public Weapon Weapon;
        [MarshalAs(UnmanagedType.I4)] public int StartTick;
    }
}
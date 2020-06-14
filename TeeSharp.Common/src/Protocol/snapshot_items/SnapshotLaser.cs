using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class SnapshotLaser : BaseSnapshotItem
    {
        public override SnapshotItems Type => SnapshotItems.Laser;

        [MarshalAs(UnmanagedType.I4)] public int X;
        [MarshalAs(UnmanagedType.I4)] public int Y;
        [MarshalAs(UnmanagedType.I4)] public int FromX;
        [MarshalAs(UnmanagedType.I4)] public int FromY;
        [MarshalAs(UnmanagedType.I4)] public int StartTick;
    }
}
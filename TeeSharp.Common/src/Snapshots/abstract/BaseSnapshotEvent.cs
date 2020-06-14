using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;

namespace TeeSharp.Common.Snapshots
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public abstract class BaseSnapshotEvent : BaseSnapshotItem
    {
        public override SnapshotItems Type => SnapshotItems.EventCommon;

        [MarshalAs(UnmanagedType.I4)] public int X;
        [MarshalAs(UnmanagedType.I4)] public int Y;
    }
}
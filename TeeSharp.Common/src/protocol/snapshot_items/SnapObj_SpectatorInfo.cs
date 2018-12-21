using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class SnapObj_SpectatorInfo : BaseSnapObject
    {
        public override SnapshotItems Type => SnapshotItems.SpectatorInfo;

        [MarshalAs(UnmanagedType.I4)] public SpectatorMode SpectatorMode;
        [MarshalAs(UnmanagedType.I4)] public int SpectatorId;
        [MarshalAs(UnmanagedType.I4)] public int X;
        [MarshalAs(UnmanagedType.I4)] public int Y;
    }
}
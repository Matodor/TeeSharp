using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class SnapshotDemoGameInfo : BaseSnapshotItem
    {
        public override SnapshotItems Type => SnapshotItems.DemoGameInfo;

        [MarshalAs(UnmanagedType.I4)] public GameFlags GameFlags;
        [MarshalAs(UnmanagedType.I4)] public int ScoreLimit;
        [MarshalAs(UnmanagedType.I4)] public int TimeLimit;
        [MarshalAs(UnmanagedType.I4)] public int MatchNum;
        [MarshalAs(UnmanagedType.I4)] public int MatchCurrent;
    }
}
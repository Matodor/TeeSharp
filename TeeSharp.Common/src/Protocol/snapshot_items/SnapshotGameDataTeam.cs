using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class SnapshotGameDataTeam : BaseSnapshotItem
    {
        public override SnapshotItems Type => SnapshotItems.GameDataTeam;

        [MarshalAs(UnmanagedType.I4)] public int ScoreRed;
        [MarshalAs(UnmanagedType.I4)] public int ScoreBlue;
    }
}
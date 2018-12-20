using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class SnapObj_GameDataTeam : BaseSnapObject
    {
        public override SnapshotObjects Type => SnapshotObjects.GameDataTeam;

        [MarshalAs(UnmanagedType.I4)] public int ScoreRed;
        [MarshalAs(UnmanagedType.I4)] public int ScoreBlue;
    }
}
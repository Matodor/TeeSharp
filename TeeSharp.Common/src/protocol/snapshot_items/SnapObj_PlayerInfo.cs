using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class SnapObj_PlayerInfo : BaseSnapObject
    {
        public override SnapshotItems Type => SnapshotItems.PlayerInfo;

        [MarshalAs(UnmanagedType.I4)] public PlayerFlags PlayerFlags;
        [MarshalAs(UnmanagedType.I4)] public int Score;
        [MarshalAs(UnmanagedType.I4)] public int Latency;
    }
}
using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class SnapshotGameDataFlag  : BaseSnapshotItem
    {
        public override SnapshotItems Type => SnapshotItems.GameDataFlag;

        [MarshalAs(UnmanagedType.I4)] public int FlagCarrierRed;
        [MarshalAs(UnmanagedType.I4)] public int FlagCarrierBlue;
        [MarshalAs(UnmanagedType.I4)] public int FlagDropTickRed;
        [MarshalAs(UnmanagedType.I4)] public int FlagDropTickBlue;
    }
}
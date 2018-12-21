using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class SnapObj_DemoTuneParams : BaseSnapObject
    {
        public override SnapshotItems Type => SnapshotItems.DemoTuneParams;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public int[] TuneParams;
    }
}
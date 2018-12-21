using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;

namespace TeeSharp.Common.Protocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class SnapObj_DemoClientInfo : BaseSnapObject
    {
        public override SnapshotItems Type => SnapshotItems.DemoClientInfo;

        public bool IsLocal
        {
            get => Local != 0;
            set => Local = value ? 1 : 0;
        }

        [MarshalAs(UnmanagedType.I4)] public int Local;
        [MarshalAs(UnmanagedType.I4)] public Team Team;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public int[] IntName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public int[] IntClan;

        [MarshalAs(UnmanagedType.I4)] public int Country;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public int[] SkinPartNames1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public int[] SkinPartNames2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public int[] SkinPartNames3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public int[] SkinPartNames4;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public int[] SkinPartNames5;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public int[] SkinPartNames6;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public int[] UseCustomColors;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public int[] SkinPartColors;
    }
}
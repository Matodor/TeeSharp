using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Snapshots;
using TeeSharp.Core;

namespace TeeSharp.Common.Protocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class SnapshotDemoClientInfo : BaseSnapshotItem
    {
        public override SnapshotItems Type => SnapshotItems.DemoClientInfo;

        public bool IsLocal
        {
            get => Local != 0;
            set => Local = value ? 1 : 0;
        }

        public string Name
        {
            get => IntName.IntsToStr();
            set => IntName = value.StrToInts(4);
        }

        public string Clan
        {
            get => IntClan.IntsToStr();
            set => IntClan = value.StrToInts(3);
        }

        public string SkinPartName1
        {
            get => IntSkinPartName1.IntsToStr();
            set => IntSkinPartName1 = value.StrToInts(6);
        }

        public string SkinPartName2
        {
            get => IntSkinPartName2.IntsToStr();
            set => IntSkinPartName2 = value.StrToInts(6);
        }

        public string SkinPartName3
        {
            get => IntSkinPartName3.IntsToStr();
            set => IntSkinPartName3 = value.StrToInts(6);
        }

        public string SkinPartName4
        {
            get => IntSkinPartName4.IntsToStr();
            set => IntSkinPartName4 = value.StrToInts(6);
        }

        public string SkinPartName5
        {
            get => IntSkinPartName5.IntsToStr();
            set => IntSkinPartName5 = value.StrToInts(6);
        }

        public string SkinPartName6
        {
            get => IntSkinPartName6.IntsToStr();
            set => IntSkinPartName6 = value.StrToInts(6);
        }

        [MarshalAs(UnmanagedType.I4)] public int Local;
        [MarshalAs(UnmanagedType.I4)] public Team Team;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public int[] IntName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public int[] IntClan;

        [MarshalAs(UnmanagedType.I4)] public int Country;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public int[] IntSkinPartName1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public int[] IntSkinPartName2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public int[] IntSkinPartName3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public int[] IntSkinPartName4;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public int[] IntSkinPartName5;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public int[] IntSkinPartName6;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public int[] UseCustomColors;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public int[] SkinPartColors;
    }
}
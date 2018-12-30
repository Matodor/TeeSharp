using System;
using System.Runtime.InteropServices;
using TeeSharp.Common.Enums;
using TeeSharp.Common.Game;
using TeeSharp.Common.Snapshots;
using TeeSharp.Core;

namespace TeeSharp.Common.Protocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class SnapshotDemoClientInfo : BaseSnapshotItem
    {
        public override SnapshotItems Type => SnapshotItems.DemoClientInfo;

        public SkinPartParams this[SkinPart part]
        {
            set
            {
                var index = 0;
                switch (part)
                {
                    case SkinPart.Body:
                        IntSkinPartName1 = value.Name.StrToInts(6);
                        break;
                    case SkinPart.Marking:
                        IntSkinPartName2 = value.Name.StrToInts(6);
                        break;
                    case SkinPart.Decoration:
                        IntSkinPartName3 = value.Name.StrToInts(6);
                        break;
                    case SkinPart.Hands:
                        IntSkinPartName4 = value.Name.StrToInts(6);
                        break;
                    case SkinPart.Feet:
                        IntSkinPartName5 = value.Name.StrToInts(6);
                        break;
                    case SkinPart.Eyes:
                        IntSkinPartName6 = value.Name.StrToInts(6);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(part), part, null);
                }

                SkinPartUseCustomColors[(int) part] = value.UseCustomColor ? 1 : 0;
                SkinPartColors[(int)part] = value.Color;
            }
        }

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
        
        [MarshalAs(UnmanagedType.I4)] public int Local;
        [MarshalAs(UnmanagedType.I4)] public Team Team;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public int[] IntName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)] public int[] IntClan;

        [MarshalAs(UnmanagedType.I4)] public int Country;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) SkinPart.NumParts)] public int[] IntSkinPartName1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) SkinPart.NumParts)] public int[] IntSkinPartName2;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) SkinPart.NumParts)] public int[] IntSkinPartName3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) SkinPart.NumParts)] public int[] IntSkinPartName4;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) SkinPart.NumParts)] public int[] IntSkinPartName5;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) SkinPart.NumParts)] public int[] IntSkinPartName6;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) SkinPart.NumParts)] public int[] SkinPartUseCustomColors;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) SkinPart.NumParts)] public int[] SkinPartColors;
    }
}
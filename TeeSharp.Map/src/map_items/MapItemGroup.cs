using System.Runtime.InteropServices;

namespace TeeSharp.Map.MapItems
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct MapItemGroup
    {
        public const int CURRENT_VERSION = 3;

        [MarshalAs(UnmanagedType.I4)]
        public int Version;

        [MarshalAs(UnmanagedType.I4)]
        public int OffsetX;

        [MarshalAs(UnmanagedType.I4)]
        public int OffsetY;

        [MarshalAs(UnmanagedType.I4)]
        public int ParallaxX;

        [MarshalAs(UnmanagedType.I4)]
        public int ParallaxY;
        
        [MarshalAs(UnmanagedType.I4)]
        public int StartLayer;

        [MarshalAs(UnmanagedType.I4)]
        public int NumLayers;

        [MarshalAs(UnmanagedType.I4)]
        public int UseClipping;

        [MarshalAs(UnmanagedType.I4)]
        public int ClipX;

        [MarshalAs(UnmanagedType.I4)]
        public int ClipY;

        [MarshalAs(UnmanagedType.I4)]
        public int ClipW;

        [MarshalAs(UnmanagedType.I4)]
        public int ClipH;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public int[] IntName;
    }
}
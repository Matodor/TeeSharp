using System.Runtime.InteropServices;

namespace TeeSharp.Map.MapItems
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct MapItemLayerQuads
    {
        public MapItemLayer Layer;

        [MarshalAs(UnmanagedType.I4)]
        public int Version;

        [MarshalAs(UnmanagedType.I4)]
        public int NumQuads;

        [MarshalAs(UnmanagedType.I4)]
        public int Data;

        [MarshalAs(UnmanagedType.I4)]
        public int Image;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public int[] IntName;
    }
}
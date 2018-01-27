using System.Runtime.InteropServices;

namespace TeeSharp.Map.MapItems
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class MapItemLayerTilemap
    {
        public MapItemLayer Layer;
        
        [MarshalAs(UnmanagedType.I4)]
        public int Version;

        [MarshalAs(UnmanagedType.I4)]
        public int Width;

        [MarshalAs(UnmanagedType.I4)]
        public int Height;

        [MarshalAs(UnmanagedType.I4)]
        public int Flags;

        public Color Color;

        [MarshalAs(UnmanagedType.I4)]
        public int ColorEnv;

        [MarshalAs(UnmanagedType.I4)]
        public int ColorEnvOffset;

        [MarshalAs(UnmanagedType.I4)]
        public int Image;

        [MarshalAs(UnmanagedType.I4)]
        public int Data;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public int[] IntName;
    }
}
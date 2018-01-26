using System.Runtime.InteropServices;

namespace TeeSharp.Map.MapItems
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct Quad
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public Point[] Points;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public Color[] Colors;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public Point[] TexCoords;

        [MarshalAs(UnmanagedType.I4)]
        public int PosEnv;

        [MarshalAs(UnmanagedType.I4)]
        public int PosEnvOffset;

        [MarshalAs(UnmanagedType.I4)]
        public int ColorEnv;

        [MarshalAs(UnmanagedType.I4)]
        public int ColorEnvOffset;
    }
}
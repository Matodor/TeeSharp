using System.Runtime.InteropServices;

namespace TeeSharp.Map.MapItems
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct Tile
    {
        [MarshalAs(UnmanagedType.I1)]
        public byte Index;

        [MarshalAs(UnmanagedType.I1)]
        public byte Flags;

        [MarshalAs(UnmanagedType.I1)]
        public byte Skip;

        [MarshalAs(UnmanagedType.I1)]
        public byte Reserved;
    }
}
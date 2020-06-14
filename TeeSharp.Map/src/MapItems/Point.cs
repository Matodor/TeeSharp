using System.Runtime.InteropServices;

namespace TeeSharp.Map.MapItems
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct Point
    {
        [MarshalAs(UnmanagedType.I4)]
        public int X; // 22.10 fixed point

        [MarshalAs(UnmanagedType.I4)]
        public int Y; // 22.10 fixed point
    }
}
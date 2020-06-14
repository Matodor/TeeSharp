using System.Runtime.InteropServices;

namespace TeeSharp.Map.MapItems
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct Color
    {
        [MarshalAs(UnmanagedType.I4)] public int R;
        [MarshalAs(UnmanagedType.I4)] public int G;
        [MarshalAs(UnmanagedType.I4)] public int B;
        [MarshalAs(UnmanagedType.I4)] public int A;
    }
}
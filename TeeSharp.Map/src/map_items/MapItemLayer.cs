using System.Runtime.InteropServices;

namespace TeeSharp.Map.MapItems
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class MapItemLayer
    {
        [MarshalAs(UnmanagedType.I4)]
        public int Version;

        [MarshalAs(UnmanagedType.I4)]
        public LayerType Type;

        [MarshalAs(UnmanagedType.I4)]
        public int Flags;
    }
}
using System.Runtime.InteropServices;

namespace TeeSharp.Map.map_items
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct MapItemInfo
    {
        [MarshalAs(UnmanagedType.I4)]
        public int Version;

        [MarshalAs(UnmanagedType.I4)]
        public int Author;

        [MarshalAs(UnmanagedType.I4)]
        public int MapVersion;

        [MarshalAs(UnmanagedType.I4)]
        public int Credits;

        [MarshalAs(UnmanagedType.I4)]
        public int License;
    }
}
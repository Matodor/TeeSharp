using System.Runtime.InteropServices;

namespace TeeSharp.Map.map_items
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct MapItemVersion
    {
        [MarshalAs(UnmanagedType.I4)]
        public int Version;
    }
}
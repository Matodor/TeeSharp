using System.Runtime.InteropServices;

namespace TeeSharp.Map.MapItems
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class MapItemVersion
    {
        [MarshalAs(UnmanagedType.I4)]
        public int Version;
    }
}
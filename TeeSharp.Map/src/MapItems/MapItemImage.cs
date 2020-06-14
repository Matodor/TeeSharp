using System.Runtime.InteropServices;

namespace TeeSharp.Map.MapItems
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public class MapItemImage
    {
        [MarshalAs(UnmanagedType.I4)]
        public int Version;

        [MarshalAs(UnmanagedType.I4)]
        public int Width;

        [MarshalAs(UnmanagedType.I4)]
        public int Height;

        [MarshalAs(UnmanagedType.I4)]
        public int External;

        [MarshalAs(UnmanagedType.I4)]
        public int ImageName;

        [MarshalAs(UnmanagedType.I4)]
        public int ImageData;

        [MarshalAs(UnmanagedType.I4)]
        public int Format;
    }
}
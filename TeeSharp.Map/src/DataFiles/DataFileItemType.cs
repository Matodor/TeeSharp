using System.Runtime.InteropServices;

namespace TeeSharp.Map
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct DataFileItemType
    {
        [MarshalAs(UnmanagedType.I4)]
        public MapItemType Type;

        [MarshalAs(UnmanagedType.I4)]
        public int Start;

        [MarshalAs(UnmanagedType.I4)]
        public int Num;
    }
}
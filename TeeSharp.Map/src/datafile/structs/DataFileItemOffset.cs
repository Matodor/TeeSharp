using System.Runtime.InteropServices;

namespace TeeSharp.Map
{
    /// <summary>
    /// Each item offset is the offset of the item with the corresponding index, relative to the first item's position in the file.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct DataFileItemOffset
    {
        [MarshalAs(UnmanagedType.I4)]
        public int Offset;
    }
}
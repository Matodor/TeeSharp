using System.Runtime.InteropServices;

namespace TeeSharp.Map
{
    /// <summary>
    /// The header specific to version 3 and 4 consists of seven 32-bit signed integers.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct DataFileHeader
    {
        /// <summary>
        /// The size is a little-endian integer and must be the size of the complete datafile without the version_header and both size and swaplen.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int Size;

        /// <summary>
        /// The swaplen is a little-endian integer and must specify the number of integers following little-endian integers. It can therefore be used to reverse the endian on big-endian machines.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int SwapLen;

        /// <summary>
        /// The num_item_types integer specifies the number of item types in the datafile.item_types field.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int NumItemTypes;

        /// <summary>
        /// The num_items integer specifies the number of items in the datafile.items field.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int NumItems;

        /// <summary>
        /// The num_data integer specifies the number of raw data blocks in the datafile.data field.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int NumData;

        /// <summary>
        /// The item_size integer specifies the total size in bytes of the datafile.items field.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int ItemSize;

        /// <summary>
        /// The data_size integer specifies the total size in bytes of the datafile.data field.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int DataSize;
    }
}
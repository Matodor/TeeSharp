using System.Runtime.InteropServices;
using TeeSharp.Core.Extensions;

namespace TeeSharp.Map
{
    /// <summary>
    /// The header specific to version 3 and 4 consists of seven 32-bit signed integers.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct DataFileHeader
    {
        public string IdString => StringExtensions.ToString(Id);

        /// <summary>
        /// 4 bytes
        /// The magic must exactly be the ASCII representations of the four characters, 
        /// 'D', 'A', 'T', 'A'. NOTE: Readers of Teeworlds datafiles should be able to read 
        /// datafiles which start with a reversed magic too, that is 'A', 'T', 'A', 'D'. 
        /// A bug in the reference implementation caused big-endian machines to save the reversed magic bytes.
        /// </summary>

        //[FieldOffset(0)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] Id;

        /// <summary>
        /// 4 bytes
        /// The version is a little-endian signed 32-bit integer, for version 
        /// 3 or 4 of Teeworlds datafiles, it must be 3 or 4, respectively.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int Version;

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
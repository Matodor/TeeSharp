using System.Runtime.InteropServices;

namespace TeeSharp.Map
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct DataFileItem
    {
        /// <summary>
        /// The type_id__id integer consists of 16 bit type_id of the type the item belongs to and 16 bit id that uniquely identifies the item among all others of the same type, in that order.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int TypeIdAndItemId;

        /// <summary>
        /// The size signed 32-bit integer is the size of the item_data field, in bytes, which must be divisible by four.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int Size;
    }
}
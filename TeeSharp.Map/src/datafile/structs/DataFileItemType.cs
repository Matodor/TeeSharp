using System.Runtime.InteropServices;

namespace TeeSharp.Map
{
    /// <summary>
    /// The item types are an array of item types. The number of item types in that array is num_item_types, each item type is identified by its unique type_id (explained below).
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct DataFileItemType
    {
        /// <summary>
        /// The type_id 32-bit signed integer must be unique amongst all other item_type.type_ids. Its value must fit into an unsigned 16-bit integer.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public MapItemTypes TypeId;

        /// <summary>
        /// The start signed integer is the index of the first item in the items with the type type_id.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int Start;

        /// <summary>
        /// The num signed integer must be the number of items with the the type type_id.
        /// </summary>
        [MarshalAs(UnmanagedType.I4)]
        public int Num;
    }
}
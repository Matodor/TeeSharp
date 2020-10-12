using System.Runtime.InteropServices;

namespace TeeSharp.Map
{
    public struct DataFileItem
    {
        public int Type => (TypeIdAndItemId >> 16) & 0b1111_1111_1111_1111;
        public int Id => TypeIdAndItemId & 0b1111_1111_1111_1111;
        
        public int TypeIdAndItemId;
        public int Size;
    }
}
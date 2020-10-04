using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace TeeSharp.Map
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct DataFileHeader
    {
        /*
            1096040772 = ((byte) 'D') * (1 << 0) +
                ((byte) 'A') * (1 << 8) +
                ((byte) 'T') * (1 << 16) +
                ((byte) 'A') * (1 << 24);
                
            1145132097 = ((byte) 'A') * (1 << 0) +
                ((byte) 'T') * (1 << 8) +
                ((byte) 'A') * (1 << 16) +
                ((byte) 'D') * (1 << 24);
         */
        public bool IsValidSignature => Id == 1096040772 || Id == 1145132097;
        
        public bool IsValidVersion => Version == 3 || Version == 4;

        public int Id;
        public int Version;
        public int Size;
        public int Swaplen;
        public int ItemTypesCount;
        public int ItemsCount;
        public int RawDataSize;
        public int ItemSize;
        public int DataSize;
    }
}
using System;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using TeeSharp.Core.Extensions;

namespace TeeSharp.Map
{
    public unsafe struct DataFileHeader
    {
        public Span<byte> Signature => new Span<byte>(Unsafe.AsPointer(ref _signature[0]), 4);

        public bool IsValidSignature => 
            Encoding.ASCII.GetString(Signature) == "DATA" || 
            Encoding.ASCII.GetString(Signature) == "ATAD"; 
        
        public bool IsValidVersion => Version == 4;
        
        private fixed byte _signature[4];
        
        public int Version;
        public int Size;
        public int SwapLength;
        public int ItemTypesCount;
        public int ItemsCount;
        public int RawDataBlocks;
        public int ItemsSize;
        public int RawDataBlocksSize;
    }
}
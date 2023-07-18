using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace TeeSharp.Benchmark;

public class DeserializeMapHeaderBenchmark
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private unsafe struct Header1
    {
        public fixed byte Signature[4];

        public int Version;
        public int Size;
        public int SwapLength;
        public int NumberOfItemTypes;
        public int NumberOfItems;
        public int NumberOfRawDataBlocks;
        public int ItemsSize;
        public int RawDataBlocksSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct Header2
    {
        private byte _signature1;
        private byte _signature2;
        private byte _signature3;
        private byte _signature4;

        public int Version;
        public int Size;
        public int SwapLength;
        public int NumberOfItemTypes;
        public int NumberOfItems;
        public int NumberOfRawDataBlocks;
        public int ItemsSize;
        public int RawDataBlocksSize;
    }

    private readonly byte[] _header = {
        68, 65, 84, 65, 4, 0,
        0, 0, 215, 93, 4, 0,
        212, 11, 0, 0, 6, 0,
        0, 0, 34, 0, 0, 0,
        41, 0, 0, 0, 168, 9,
        0, 0, 3, 82, 4, 0,
    };

    [Benchmark(Description = "Fixed array: Marshal.PtrToStructure")]
    public void FixedArray_MarshalPtrToStructure()
    {
        var handle = GCHandle.Alloc(_header, GCHandleType.Pinned);
        var header = Marshal.PtrToStructure<Header1>(handle.AddrOfPinnedObject());
        handle.Free();
    }

    [Benchmark(Description = "Fixed array: MemoryMarshal.Read")]
    public void FixedArray_MemoryMarshalRead()
    {
        var header = MemoryMarshal.Read<Header1>(_header);
    }

    [Benchmark(Description = "Fixed array: Unsafe.ReadUnaligned")]
    public void FixedArray_UnsafeReadUnaligned()
    {
        var header = Unsafe.ReadUnaligned<Header1>(ref MemoryMarshal.GetReference(_header.AsSpan()));
    }

    [Benchmark(Description = "4 fields: Marshal.PtrToStructure")]
    public void FourFields_MarshalPtrToStructure()
    {
        var handle = GCHandle.Alloc(_header, GCHandleType.Pinned);
        var header = Marshal.PtrToStructure<Header2>(handle.AddrOfPinnedObject());
        handle.Free();
    }

    [Benchmark(Description = "4 fields: MemoryMarshal.Read")]
    public void FourFields_MemoryMarshalRead()
    {
        var header = MemoryMarshal.Read<Header2>(_header);
    }

    [Benchmark(Description = "4 fields: Unsafe.ReadUnaligned")]
    public void FourFields_UnsafeReadUnaligned()
    {
        var header = Unsafe.ReadUnaligned<Header2>(ref MemoryMarshal.GetReference(_header.AsSpan()));
    }
}

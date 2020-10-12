using System;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace TeeSharp.Benchmark
{
    public class DeserializeStructWithArrayBenchmark
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct Struct1
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] IpData;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] PortData;
        }
        
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private unsafe struct Struct2
        {
            public fixed byte IpDate[16];
            public fixed byte Port[2];
        }
        
        [Benchmark(Description = "Marshal.PtrToStructure")]
        public void MarshalPtrToStructure()
        {
            var data = new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111};
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var struct1 = Marshal.PtrToStructure<Struct1>(handle.AddrOfPinnedObject());
            handle.Free();
        }

        [Benchmark(Description = "MemoryMarshal.Read")]
        public void MemoryMarshalRead()
        {
            var data = new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111};
            var addr = MemoryMarshal.Read<Struct2>(data.AsSpan());
        }
    }
}
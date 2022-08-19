using System;
using System.Net;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using TeeSharp.Core.Helpers;
using TeeSharp.Network;

namespace TeeSharp.Benchmark;

public class DeserializeBenchmark
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct DataFileHeader1
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] Id;

        public int Version;
        public int Size;
        public int Swaplen;
        public int NumItemTypes;
        public int NumItems;
        public int NumRawData;
        public int ItemSize;
        public int DataSize;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct TestStruct1
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] IpData;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] PortData;

        public static explicit operator IPEndPoint(TestStruct1 serverEndpoint)
        {
            var buffer = serverEndpoint.IpData.AsSpan();
            var ipV4Mapping = new Span<byte>(new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255});
            var isIpV4 = ipV4Mapping.SequenceEqual(buffer.Slice(0, 12));
            var port = (serverEndpoint.PortData[0] << 8) | serverEndpoint.PortData[1];

            return new IPEndPoint(new IPAddress(isIpV4 ? buffer.Slice(12, 4) : buffer), port);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct TestStruct2
    {
        private readonly int _ipData1;
        private readonly int _ipData2;
        private readonly int _ipData3;
        private readonly int _ipData4;
        private readonly byte _portData1;
        private readonly byte _portData2;

        public static explicit operator IPEndPoint(TestStruct2 endpoint)
        {
            var buffer = MemoryMarshal.Cast<int, byte>(new Span<int>(new int[]
            {
                endpoint._ipData1,
                endpoint._ipData2,
                endpoint._ipData3,
                endpoint._ipData4,
            }));

            var isIpV4 = NetworkConstants.IpV4Mapping.AsSpan().SequenceEqual(buffer.Slice(0, 12));
            var port = (endpoint._portData1 << 8) | endpoint._portData2;

            return new IPEndPoint(new IPAddress(isIpV4 ? buffer.Slice(12, 4) : buffer), port);
        }
    }

    private static class Struct3
    {
        public static IPEndPoint Get(Span<byte> data)
        {
            var isIpV4 = NetworkConstants.IpV4Mapping.AsSpan().SequenceEqual(data.Slice(0, 12));
            var port = (data[16] << 8) | data[17];

            return new IPEndPoint(new IPAddress(isIpV4 ? data.Slice(12, 4) : data), port);
        }

        public static IPEndPoint[] GetArray(Span<byte> data)
        {
            const int sizeOfServerEndpoint = 18;
            var array = new IPEndPoint[data.Length / sizeOfServerEndpoint];

            for (var i = 0; i < array.Length; i++)
                array[i] = Get(data.Slice(i * sizeOfServerEndpoint));

            return array;
        }
    }

    [Benchmark(Description = "IPEndPoint Marshal.PtrToStructure")]
    public void MarshalPtrToStructure()
    {
        var data = new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111};
        var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        var addr = Marshal.PtrToStructure<TestStruct1>(handle.AddrOfPinnedObject());
        var endpoint = (IPEndPoint) addr;
        handle.Free();
    }

    [Benchmark(Description = "IPEndPoint MemoryMarshal.Cast")]
    public void MemoryMarshalCast()
    {
        var data = new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111};
        var addr = MemoryMarshal.Cast<byte, TestStruct2>(data.AsSpan())[0];
        var endpoint = (IPEndPoint) addr;
    }

    [Benchmark(Description = "IPEndPoint MemoryMarshal.AsBytes")]
    public void MemoryMarshalAsBytes()
    {
        var data = new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111};
        var addr = default(TestStruct2);
        var typeSpan = MemoryMarshal.CreateSpan(ref addr, 1);
        var span = MemoryMarshal.AsBytes(typeSpan);
        data.AsSpan().CopyTo(span);
        var endpoint = (IPEndPoint) addr;
    }

    [Benchmark(Description = "IPEndPoint MemoryMarshal.Read")]
    public void MemoryMarshalRead()
    {
        var data = new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111};
        var addr = MemoryMarshal.Read<TestStruct2>(data.AsSpan());
        var endpoint = (IPEndPoint) addr;
    }

    [Benchmark(Description = "IPEndPoint ForwardCast")]
    public void ForwardCast()
    {
        var data = new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111};
        var addr = Struct3.Get(data);
    }

    [Benchmark(Description = "MemoryMarshal.Read.Multiple")]
    public void MemoryMarshalReadMultiple()
    {
        var data = new byte[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
        }.AsSpan();

        var size = StructHelper<TestStruct2>.Size;
        var endPoints = new IPEndPoint[data.Length / size];

        for (var i = 0; i < endPoints.Length; i++)
            endPoints[i] = (IPEndPoint)MemoryMarshal.Read<TestStruct2>(data.Slice(i * size, size));
    }

    [Benchmark(Description = "MemoryMarshal.Cast.Multiple")]
    public void MemoryMarshalCastMultiple()
    {
        var data = new byte[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
        };

        var array = MemoryMarshal.Cast<byte, TestStruct2>(data.AsSpan()).ToArray();
        var endPoints = new IPEndPoint[array.Length];

        for (var i = 0; i < endPoints.Length; i++)
            endPoints[i] = (IPEndPoint)array[i];
    }

    [Benchmark(Description = "Struct3GetArray")]
    public void Struct3GetArray()
    {
        var data = new byte[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
        };

        var array = Struct3.GetArray(data);
    }
}

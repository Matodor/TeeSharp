using System;
using System.Net;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using TeeSharp.Network;

namespace TeeSharp.Benchmark
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct ServerEndpoint3
    {
        private readonly int _ipData1;
        private readonly int _ipData2;
        private readonly int _ipData3;
        private readonly int _ipData4;
        private readonly byte _portData1;
        private readonly byte _portData2;

        public IPEndPoint Cast1()
        {
            var buffer = MemoryMarshal.Cast<int, byte>(new Span<int>(new int[]
            {
                _ipData1,
                _ipData2,
                _ipData3,
                _ipData4,
            }));
            
            var isIpV4 = NetworkConstants.IpV4Mapping.AsSpan().SequenceEqual(buffer.Slice(0, 12));
            var port = (_portData1 << 8) | _portData2;
            
            return new IPEndPoint(new IPAddress(isIpV4 ? buffer.Slice(12, 4) : buffer), port);
        }
        
        public IPEndPoint Cast2()
        {
            var buffer = MemoryMarshal.AsBytes(new Span<ServerEndpoint3>(new[] {this}));
            var isIpV4 = NetworkConstants.IpV4Mapping.AsSpan().SequenceEqual(buffer.Slice(0, 12));
            var port = (_portData1 << 8) | _portData2;
            
            return new IPEndPoint(new IPAddress(isIpV4 ? buffer.Slice(12, 4) : buffer), port);
        }

        public static explicit operator IPEndPoint(ServerEndpoint3 endpoint)
        {
            var span = MemoryMarshal.CreateSpan(ref endpoint, 1);
            var buffer = MemoryMarshal.Cast<ServerEndpoint3, byte>(span);
            var isIpV4 = NetworkConstants.IpV4Mapping.AsSpan().SequenceEqual(buffer.Slice(0, 12));
            var port = (endpoint._portData1 << 8) | endpoint._portData2;
            
            return new IPEndPoint(new IPAddress(isIpV4 ? buffer.Slice(12, 4) : buffer), port);
        }
    }
    
    public class CastIpEndpointBenchmark
    {
        [Benchmark(Description = "Cast1")]
        public void Cast1()
        {
            for (int i = 0; i < 1000; i++)
            {
                var data = new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111};
                var addr = MemoryMarshal.Read<ServerEndpoint3>(data.AsSpan());
                var endpoint = addr.Cast1();
                // var addr = MemoryMarshal.Cast<byte, ServerEndpoint3>(data.AsSpan())[0];
            }
        }
        
        [Benchmark(Description = "Cast2")]
        public void Cast2()
        {
            for (int i = 0; i < 1000; i++)
            {
                var data = new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111};
                var addr = MemoryMarshal.Read<ServerEndpoint3>(data.AsSpan());
                var endpoint = addr.Cast2();
            }
        }
        
        [Benchmark(Description = "Cast3")]
        public void Cast3()
        {
            for (int i = 0; i < 1000; i++)
            {
                var data = new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111};
                var addr = MemoryMarshal.Read<ServerEndpoint3>(data.AsSpan());
                var endpoint = (IPEndPoint) addr;
            }
        }
    }
}
using System.Net;
using BenchmarkDotNet.Attributes;
using TeeSharp.Network.Extensions;

namespace TeeSharp.Benchmark
{
    public class EndPointCompareBenchmark
    {
        [Benchmark(Description = "CompareNotEqual1")]
        public void CompareNotEqual1()
        {
            for (var i = 0; i < 100000; i++)
            {
                var endPoint1 = new IPEndPoint(IPAddress.Parse("192.168.137.106"), 51850);
                var endPoint2 = new IPEndPoint(IPAddress.Parse("188.13.68.77"), 8303);
                var equals = endPoint1.Compare(endPoint2, true);
            }
        }

        [Benchmark(Description = "CompareNotEqual2")]
        public void CompareNotEqual2()
        {
            for (var i = 0; i < 100000; i++)
            {
                var endPoint1 = new IPEndPoint(IPAddress.Parse("192.168.137.106"), 51850);
                var endPoint2 = new IPEndPoint(IPAddress.Parse("188.13.68.77"), 8303);
                var equals = Compare2(endPoint1, endPoint2, true);
            }
        }

        [Benchmark(Description = "CompareEqual1")]
        public void CompareEqual1()
        {
            for (var i = 0; i < 100000; i++)
            {
                var endPoint1 = new IPEndPoint(IPAddress.Parse("192.168.137.106"), 51850);
                var endPoint2 = new IPEndPoint(IPAddress.Parse("192.168.137.106"), 51850);
                var equals = endPoint1.Compare(endPoint2, true);
            }
        }

        [Benchmark(Description = "CompareEqual2")]
        public void CompareEqual2()
        {
            for (var i = 0; i < 100000; i++)
            {
                var endPoint1 = new IPEndPoint(IPAddress.Parse("192.168.137.106"), 51850);
                var endPoint2 = new IPEndPoint(IPAddress.Parse("192.168.137.106"), 51850);
                var equals = Compare2(endPoint1, endPoint2, true);
            }
        }

        private static bool Compare2(IPEndPoint endPoint1, IPEndPoint endPoint2, bool comparePorts)
        {
            if (comparePorts && endPoint1.Port != endPoint2.Port)
                return false;
            
            if (endPoint1.Address.AddressFamily != endPoint2.AddressFamily)
                return false;

            var b1 = endPoint1.Address.GetAddressBytes();
            var b2 = endPoint2.Address.GetAddressBytes();

            if (b1.Length != b2.Length)
                return false;

            for (var i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i])
                    return false;
            }

            return true;
        }
    }
}
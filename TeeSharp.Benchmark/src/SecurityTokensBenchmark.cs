using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using TeeSharp.Core.Helpers;

namespace TeeSharp.Benchmark
{
    public class SecurityTokensBenchmark
    {
        private byte[] _seed;
        private IPEndPoint _endPoint;
        
        private const int SeedSize = 12;
        
        [GlobalSetup]
        public void Setup()
        {
            _seed = new byte[SeedSize];
            _endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8303);
            RandomNumberGenerator.Create().GetBytes(_seed);

            GetTokenWithKnuthHashByteArray();
            GetTokenWithKnuthHash();
        }
        
        [Benchmark(Description = "GetTokenWithKnuthHash")]
        public void GetTokenWithKnuthHash()
        {
            var buffer = new Span<byte>(new byte[sizeof(int) + SeedSize]);
            Unsafe.As<byte, int>(ref buffer[0]) = _endPoint.GetHashCode();
            _seed.CopyTo(buffer.Slice(sizeof(int)));
            
            var knuthHash = SecurityHelper.KnuthHash(buffer);
            var token = knuthHash.GetHashCode();
        }
        
        [Benchmark(Description = "GetTokenWithKnuthHashByteArray")]
        public void GetTokenWithKnuthHashByteArray()
        {
            var addressBytes = _endPoint.Address.GetAddressBytes().AsSpan();
            var buffer = new Span<byte>(new byte[addressBytes.Length + sizeof(int) + SeedSize]);
            
            addressBytes.CopyTo(buffer);
            Unsafe.As<byte, int>(ref buffer[addressBytes.Length]) = _endPoint.Port;
            _seed.CopyTo(buffer.Slice(addressBytes.Length + sizeof(int)));
            
            var knuthHash = SecurityHelper.KnuthHash(buffer);
            var token = knuthHash.GetHashCode();
        }
        
        [Benchmark(Description = "GetTokenWithSha256")]
        public void GetTokenWithSha256()
        {
            var buffer = new Span<byte>(new byte[sizeof(int) + SeedSize]);
            Unsafe.As<byte, int>(ref buffer[0]) = _endPoint.GetHashCode();
            _seed.CopyTo(buffer.Slice(sizeof(int)));

            var hash = SHA256.HashData(buffer);
            var token = BitConverter.ToInt32(hash);
        }
    }
}
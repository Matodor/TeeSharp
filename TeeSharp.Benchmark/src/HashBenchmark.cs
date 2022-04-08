using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using BenchmarkDotNet.Attributes;
using TeeSharp.Core.Helpers;

namespace TeeSharp.Benchmark;

public class HashBenchmark
{
    private byte[] _buffer;
    private const int SeedSize = 16;
        
    [GlobalSetup]
    public void Setup()
    {
        _buffer = new byte[SeedSize + sizeof(int)];
        RandomNumberGenerator.Create().GetBytes(_buffer.AsSpan(0, SeedSize));
        Unsafe.As<byte, int>(ref _buffer[SeedSize]) = 
            new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8303).GetHashCode();
    }
        
    [Benchmark(Description = "KnuthHash")]
    public void KnuthHash()
    {
        var knuthHash = SecurityHelper.KnuthHash(_buffer);
    }
        
    [Benchmark(Description = "Sha256")]
    public void Sha256()
    {
        var hash = SHA256.HashData(_buffer);
    }
}
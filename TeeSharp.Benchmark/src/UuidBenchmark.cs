using System;
using BenchmarkDotNet.Attributes;
using Uuids;

namespace TeeSharp.Benchmark;

public class UuidBenchmark
{
    [Benchmark(Description = "Create UUID")]
    public void Method1()
    {
        var bytes = (Span<byte>) new byte[]
        {
            224, 93, 218, 170, 196, 230, 76, 251, 182, 66, 93, 72, 232, 12, 0, 41,
        };

        var uuid = new Uuid(bytes);
    }

    [Benchmark(Description = "Create GUID with reverse")]
    public void Method2()
    {
        var bytes = (Span<byte>) new byte[]
        {
            224, 93, 218, 170, 196, 230, 76, 251, 182, 66, 93, 72, 232, 12, 0, 41,
        };

        bytes.Slice(0, 4).Reverse();
        bytes.Slice(4, 2).Reverse();
        bytes.Slice(6, 2).Reverse();

        var guid = new Guid(bytes);
    }
}

using System;
using BenchmarkDotNet.Attributes;

namespace TeeSharp.Benchmark;

public class HasFlagBenchmark
{
    [Flags]
    public enum Flags
    {
        None = 0,
        Flag1 = 1 << 0,
        Flag2 = 1 << 1,
        Flag3 = 1 << 2,
        Flag4 = 1 << 3,
        All = None | Flag1 | Flag2 | Flag3 | Flag4,
    }

    [Benchmark(Description = "Test1")]
    public void Test1()
    {
        for (var i = 0; i < 100000; i++)
        {
            var flags = Flags.Flag1 | Flags.Flag2;
            var hasFlag = (flags & Flags.Flag2) != 0;
        }
    }
        
    [Benchmark(Description = "Test2")]
    public void Test2()
    {
        for (var i = 0; i < 100000; i++)
        {
            var flags = Flags.Flag1 | Flags.Flag2;
            var hasFlag = (flags & Flags.Flag2) == Flags.Flag2;
        }
    }
        
    [Benchmark(Description = "Test3")]
    public void Test3()
    {
        for (var i = 0; i < 100000; i++)
        {
            var flags = Flags.Flag1 | Flags.Flag2;
            var hasFlag = flags.HasFlag(Flags.Flag2);
        }
    }
}
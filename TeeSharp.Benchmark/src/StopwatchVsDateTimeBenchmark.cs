using System;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;

namespace TeeSharp.Benchmark;

public class StopwatchVsDateTimeBenchmark
{
    [Benchmark(Description = "Stopwatch")]
    public void Test1()
    {
        var time = 0L;

        for (var i = 0; i < 1000000; i++)
        {
            time = Stopwatch.GetTimestamp();
        }
    }

    [Benchmark(Description = "DateTime")]
    public void Test2()
    {
        var time = DateTime.MinValue;

        for (var i = 0; i < 1000000; i++)
        {
            time = DateTime.Now;
        }
    }
}

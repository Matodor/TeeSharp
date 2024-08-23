using System;
using BenchmarkDotNet.Attributes;

namespace TeeSharp.Benchmark;

public class ConvertEnumToIntBenchmark
{
    public enum Test
    {
        Value1,
        Value2,
    }

    [Benchmark(Description = "Cast")]
    public void Test1()
    {
        var enumValue = Test.Value1;
        int test;

        for (var i = 0; i < 1000000; i++)
        {
            test = (int)enumValue;
        }
    }

    [Benchmark(Description = "Convert.ToInt32")]
    public void Test2()
    {
        var enumValue = Test.Value1;
        int test;

        for (var i = 0; i < 1000000; i++)
        {
            test = Convert.ToInt32(enumValue);
        }
    }

    [Benchmark(Description = "GetHashCode")]
    public void Test3()
    {
        var enumValue = Test.Value1;
        int test;

        for (var i = 0; i < 1000000; i++)
        {
            test = enumValue.GetHashCode();
        }
    }
}

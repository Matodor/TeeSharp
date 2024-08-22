using BenchmarkDotNet.Attributes;

namespace TeeSharp.Benchmark;

public class FieldVsPropertyBenchmark
{
    public int Field;
    public int Property { get; set; }

    [Benchmark(Description = "Field")]
    public void Test1()
    {
        for (var i = 0; i < 1000000; i++)
        {
            Field += 1;
        }

        var a = Field;
    }

    [Benchmark(Description = "Property")]
    public void Test2()
    {
        for (var i = 0; i < 1000000; i++)
        {
            Property += 1;
        }

        var a = Property;
    }
}

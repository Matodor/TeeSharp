using BenchmarkDotNet.Attributes;

namespace TeeSharp.Benchmark;

public class ImplicitCastBenchmark
{
    private class ImplicitIntField
    {
        public int Value;

        public static implicit operator int(ImplicitIntField value)
        {
            return value.Value;
        }
    }

    private class Options
    {
        public ImplicitIntField ImplicitField { get; set; } = new ImplicitIntField();
        public int IntField { get; set; }
    }

    private Options _options = null!;

    [GlobalSetup]
    public void Setup()
    {
        _options = new Options
        {
            ImplicitField = { Value = 100 },
            IntField = 100,
        };
    }

    [Benchmark(Description = "ImplicitCast")]
    public void ImplicitCast()
    {
        var sum = 0;

        for (int i = 0; i < 1000; i++)
            sum += _options.ImplicitField;
    }

    [Benchmark(Description = "Direct")]
    public void Direct()
    {
        var sum = 0;

        for (int i = 0; i < 1000; i++)
            sum += _options.IntField;
    }
}

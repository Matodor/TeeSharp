using BenchmarkDotNet.Attributes;

namespace TeeSharp.Benchmark;

public class RefParameterVsValueType
{
    private int[] _array = null!;
    private int _index = -1;

    [GlobalSetup]
    public void Setup()
    {
        _array = new int[1];
    }

    [Benchmark(Description = "TestParameterAsValueType")]
    public void TestParameterAsValueType()
    {
        _index = 0;
        ParameterAsValueType(_index);
    }

    [Benchmark(Description = "TestParameterAsRef")]
    public void TestParameterAsRef()
    {
        _index = 0;
        ParameterAsRef(ref _index);
    }

    [Benchmark(Description = "TestParameterAsRefIn")]
    public void TestParameterAsRefIn()
    {
        _index = 0;
        ParameterAsRefIn(_index);
    }

    private void ParameterAsValueType(int i)
    {
        _array[i] = 1;
    }

    private void ParameterAsRef(ref int i)
    {
        _array[i] = 1;
    }

    private void ParameterAsRefIn(in int i)
    {
        _array[i] = 1;
    }
}

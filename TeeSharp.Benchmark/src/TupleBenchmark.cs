using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Uuids;

namespace TeeSharp.Benchmark;

public class TupleBenchmark
{
    private Dictionary<int, Action<int>> _test1 = null!;
    private Dictionary<(int, Uuid), Action<int>> _test2 = null!;

    private readonly Uuid _existUuid = new();
    private readonly Uuid _notExistUuid = new();

    [GlobalSetup]
    public void Setup()
    {
        _test1 = new Dictionary<int, Action<int>>
        {
            {
                1, TestCallback
            },
        };

        _test2 = new Dictionary<(int, Uuid), Action<int>>
        {
            {
                (1, _existUuid), TestCallback
            },
        };
    }

    private void TestCallback(int value)
    {
        var a = value + 1;
    }

    [Benchmark(Description = "IntDictionary")]
    public void Method1()
    {
        // exist key
        if (_test1.TryGetValue(1, out var method1))
            method1(50);

        // no exist key
        if (_test1.TryGetValue(2, out var method2))
            method2(50);
    }

    [Benchmark(Description = "TupleDictionary")]
    public void Method2()
    {
        // exist key
        if (_test2.TryGetValue((1, _existUuid), out var method1))
            method1(50);

        // no exist key
        if (_test2.TryGetValue((2, _notExistUuid), out var method2))
            method2(50);
    }
}

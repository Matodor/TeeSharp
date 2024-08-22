using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Uuids;

namespace TeeSharp.Benchmark;

public class CallbacksDictionaryVsListBenchmark
{
    [ParamsAllValues]
    public bool AddItems { get; set; }

    private Dictionary<int, Action> _dictionary;
    private List<(int H, int M, Action Cb)> _listTuple;

    [GlobalSetup]
    public void Setup()
    {
        _dictionary = new Dictionary<int, Action>();
        _listTuple = new List<(int, int, Action)>();

        if (AddItems)
        {
            _dictionary.Add(19 * 60 + 50, Callback);
            _dictionary.Add(20 * 60 + 20, Callback);
            _dictionary.Add(6 * 60 + 0, Callback);
            _dictionary.Add(5 * 60 + 50, Callback);

            _listTuple.Add((19, 50, Callback));
            _listTuple.Add((20, 20, Callback));
            _listTuple.Add((6, 0, Callback));
            _listTuple.Add((5, 50 , Callback));
        }
    }

    private void Callback()
    {
    }

    [Benchmark(Description = "TestDictionaryTryGetValue_HM")]
    [Arguments(20, 20)]
    [Arguments(-1, -1)]
    public void TestDictionaryTryGetValue_HM(int h, int m)
    {
        if (_dictionary.TryGetValue(h * 60 + m, out var cb))
            cb();
    }

    [Benchmark(Description = "TestDictionaryTryGetValue")]
    [Arguments(20 * 60 + 20)]
    [Arguments(-1)]
    public void TestDictionaryTryGetValue(int t)
    {
        if (_dictionary.TryGetValue(t, out var cb))
            cb();
    }

    [Benchmark(Description = "List")]
    [Arguments(20, 20)]
    [Arguments(-1, -1)]
    public void TestList(int h, int m)
    {
        for (var i = 0; i < _listTuple.Count; i++)
        {
            if (_listTuple[i].H == h && _listTuple[i].M == m)
                _listTuple[i].Cb();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Net;
using BenchmarkDotNet.Attributes;

namespace TeeSharp.Benchmark;

public class FindAddrBenchmark
{
    public const int Size = 64;

    public Dictionary<IPEndPoint, int> Dictionary1;
    public Dictionary<int, int> Dictionary2;
    public Tuple<IPEndPoint, int>[] Array1;
    public Tuple<int, int>[] Array2;
    public IPEndPoint ExistAddr;
        
    [GlobalSetup]
    public void Setup()
    {
        Console.WriteLine("SETUP");

        Dictionary1 = new Dictionary<IPEndPoint, int>(Size);
        Dictionary2 = new Dictionary<int, int>(Size);
        Array1 = new Tuple<IPEndPoint, int>[Size];
        Array2 = new Tuple<int, int>[Size];
            
        ExistAddr = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777);
            
        var port = 10000;
        for (var i = 0; i < Size - 1; i++)
        {
            var addr = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port++);
                
            Dictionary1.Add(addr, 0);
            Dictionary2.Add(addr.GetHashCode(), 0);
            Array1[i] = new Tuple<IPEndPoint, int>(addr, 0);
            Array2[i] = new Tuple<int, int>(addr.GetHashCode(), 0);
        }
            
        Dictionary1.Add(ExistAddr, 0);
        Dictionary2.Add(ExistAddr.GetHashCode(), 0);
        Array1[^1] = new Tuple<IPEndPoint, int>(ExistAddr, 0);
        Array2[^1] = new Tuple<int, int>(ExistAddr.GetHashCode(), 0);
    }
        
    [Benchmark(Description = "DictionaryFind")]
    public void DictionaryFind()
    {
        var result1 = Dictionary1.TryGetValue(ExistAddr, out _);
    }
        
    [Benchmark(Description = "DictionaryHashFind")]
    public void DictionaryHashFind()
    {
        var result1 = Dictionary2.TryGetValue(ExistAddr.GetHashCode(), out _);
    }
        
    [Benchmark(Description = "ArrayFind")]
    public void ArrayFind()
    {
        var result1 = false;
            
        for (var i = 0; i < Array1.Length; i++)
        {
            if (Array1[i].Item1.Equals(ExistAddr))
                result1 = true;
        }
    }
        
    [Benchmark(Description = "ArrayHashFind")]
    public void ArrayHashFind()
    {
        var result1 = false;
            
        for (var i = 0; i < Array2.Length; i++)
        {
            if (Array2[i].Item1 == ExistAddr.GetHashCode())
                result1 = true;
        }
    }
        
    [Benchmark(Description = "IterateDictionary")]
    public void IterateDictionary()
    {
        foreach (var pair in Dictionary2)
        {
            var test = pair.Value + 1;
        }
    }   
        
    [Benchmark(Description = "IterateDictionaryValues")]
    public void IterateDictionaryValues()
    {
        foreach (var value in Dictionary2.Values)
        {
            var test = value + 1;
        }
    }
        
    [Benchmark(Description = "IterateDictionaryKeys")]
    public void IterateDictionaryKeys()
    {
        var keys = Dictionary2.Keys;
        foreach (var key in keys)
        {
            var test = Dictionary2[key] + 1;
        }
    }
        
    [Benchmark(Description = "IterateArray")]
    public void IterateArray()
    {
        for (var i = 0; i < Array1.Length; i++)
        {
            var test = Array1[i].Item2 + 1;
        }
    }
}
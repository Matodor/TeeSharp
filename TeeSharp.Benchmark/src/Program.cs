using System;
using BenchmarkDotNet.Running;

namespace TeeSharp.Benchmark;

internal static class Program
{
    static void Main(string[] args)
    {
        // new SecurityTokensBenchmark().Setup();

        string? result;
        int number;

        var benchmarks = new[]
        {
            typeof(SerializationBenchmark),
            typeof(DeserializeBenchmark),
            typeof(DeserializeStructWithArrayBenchmark),
            typeof(SizeofBenchmark),
            typeof(CastIpEndpointBenchmark),
            typeof(VirtualCallBenchmark),
            typeof(HasFlagBenchmark),
            typeof(ProcessMessagesBenchmark),
            typeof(FindAddrBenchmark),
            typeof(HashBenchmark),
            typeof(SecurityTokensBenchmark),
            typeof(StringToBytesArrayBenchmark),
            typeof(ImplicitCastBenchmark),
            typeof(UuidBenchmark),
            typeof(TupleBenchmark),
        };

        do
        {
            Console.WriteLine("Select banchmark: ");
            for (var i = 0; i < benchmarks.Length; i++)
                Console.WriteLine($"\t{i} - {benchmarks[i].Name}");

            Console.Write("\nWrite number: ");
            result = Console.ReadLine();

        } while (!int.TryParse(result, out number) || number < 0 || number >= benchmarks.Length);

        BenchmarkRunner.Run(benchmarks[number]);
    }
}

using System;
using BenchmarkDotNet.Running;

namespace TeeSharp.Benchmark
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string result;
            int number;

            var benchmarks = new Type[]
            {
                typeof(MarshalBenchmark),
                typeof(EndPointCompareBenchmark),
                typeof(EquatableSnapshotPlayerInput),
            };

            do
            {
                Console.WriteLine("Select banchmark:");
                for (var i = 0; i < benchmarks.Length; i++)
                {
                    Console.WriteLine($"\t{i} - {benchmarks[i].Name}");
                }

                Console.Write("\nWrite number: ");
                result = Console.ReadLine();

            } while (!int.TryParse(result, out number) || number < 0 || number >= benchmarks.Length);

            BenchmarkRunner.Run(benchmarks[number]);
            Console.ReadLine();
        }
    }
}

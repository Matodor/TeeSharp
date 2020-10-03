using System;
using System.IO;
using TeeSharp.Map;

namespace Examples.Map
{
    internal static class Program
    {
        private const string MapName = "Gold Mine";

        private static void Main(string[] args)
        {
            using (var stream = File.OpenRead($"maps/{MapName}.map"))
            {
                if (stream == null)
                {
                }
                else
                {
                    var dataFile = DataFileReader.Read(stream, out var error);
                }
            }

            Console.ReadKey();
        }
    }
}
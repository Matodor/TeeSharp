using System;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace TeeSharp.Benchmark
{
    public class StringToBytesArrayBenchmark
    {
        private const string Utf8Str = "⇌⇹⟷⤄⥂⥃⥄⥈⥊⥋⥎⥐⇋⥦⥧⥨⥩⬄";

        [Benchmark(Description = "Default")]
        public void Method1()
        {
            var array = Encoding.UTF8.GetBytes(Utf8Str).AsSpan();
        }
        
        [Benchmark(Description = "UsingSpan")]
        public void Method2()
        {
            var bufferLen = Encoding.UTF8.GetMaxByteCount(Utf8Str.Length);
            var buffer = new Span<byte>(new byte[bufferLen]);
            var length = Encoding.UTF8.GetBytes(Utf8Str.AsSpan(), buffer);
            var array = buffer.Slice(0, length);
        }
    }
}
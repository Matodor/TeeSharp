using System;
using System.Collections.Concurrent;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using TeeSharp.Core;
using TeeSharp.Map;
using TeeSharp.MasterServer;

namespace TeeSharp.Benchmark
{
    public class SizeofBenchmark
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct Struct1
        {
            public int Int1;
            public int Int2;
            public int Int3;
            public int Int4;
            public int Int5;
            public int Int6;
        }
        
        [Benchmark(Description = "Marshal.SizeOf")]
        public void MarshalSizeof()
        {
            for (var i = 0; i < 1000; i++)
            {
                var size = Marshal.SizeOf(typeof(Struct1));
            }
        }
        
        [Benchmark(Description = "Unsafe.SizeOf")]
        public void UnsafeSizeof()
        {
            for (var i = 0; i < 1000; i++)
            {
                var size = Unsafe.SizeOf<Struct1>();
            }
        }
        
        [Benchmark(Description = "SizeOfHelper")]
        public void TypeHelperSizeof()
        {
            for (var i = 0; i < 1000; i++)
            {
                var size = SizeOfHelper.SizeOf(typeof(Struct1));
            }
        } 
        
        [Benchmark(Description = "TypeHelper1")]
        public void TypeHelper1()
        {
            for (var i = 0; i < 1000; i++)
            {
                var size = TypeHelper1<Struct1>.Size;
            }
        }      
        
        [Benchmark(Description = "TypeHelper2")]
        public void TypeHelper2()
        {
            for (var i = 0; i < 1000; i++)
            {
                var size = TypeHelper2<Struct1>.Size;
            }
        }
    }
    
    /*
     * https://stackoverflow.com/a/16522565
     */
    internal static class SizeOfHelper
    {
        public static int SizeOf(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            return _cache.GetOrAdd(t, t2 =>
            {
                var dm = new DynamicMethod("$", typeof(int), Type.EmptyTypes);
                var il = dm.GetILGenerator();
                il.Emit(OpCodes.Sizeof, t2);
                il.Emit(OpCodes.Ret);

                var func = (Func<int>)dm.CreateDelegate(typeof(Func<int>));
                return func();
            });
        }

        private static readonly ConcurrentDictionary<Type, int>
            _cache = new ConcurrentDictionary<Type, int>();
    }
    
    internal static class TypeHelper1<T>
    {
        public static readonly int Size = Marshal.SizeOf(typeof(T));
    }
    
    internal static class TypeHelper2<T>
    {
        public static readonly int Size = Unsafe.SizeOf<T>();
    }
}
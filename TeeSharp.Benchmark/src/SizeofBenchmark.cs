using System;
using System.Collections.Concurrent;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using TeeSharp.Core;
using TeeSharp.MasterServer;

namespace TeeSharp.Benchmark
{
    public class SizeofBenchmark
    {
        [Benchmark(Description = "Marshal.SizeOf")]
        public void MarshalSizeof()
        {
            for (var i = 0; i < 1000; i++)
            {
                var size = Marshal.SizeOf(typeof(ServerEndpoint));
            }
        }
        
        [Benchmark(Description = "Unsafe.SizeOf")]
        public void UnsafeSizeof()
        {
            for (var i = 0; i < 1000; i++)
            {
                var size = Unsafe.SizeOf<ServerEndpoint>();
            }
        }
        
        [Benchmark(Description = "TypeHelper.SizeOf")]
        public void TypeHelperSizeof()
        {
            for (var i = 0; i < 1000; i++)
            {
                var size = SizeOfHelper.SizeOf(typeof(ServerEndpoint));
            }
        } 
        
        [Benchmark(Description = "ElementSize")]
        public void ElementSize()
        {
            for (var i = 0; i < 1000; i++)
            {
                var size = TypeHelper<ServerEndpoint>.Bytes;
            }
        }
    }
    
    /*
     * https://stackoverflow.com/a/16522565
     */
    internal static class SizeOfHelper
    {
        public static int SizeOf<T>(T? obj) where T : struct
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return SizeOf(typeof(T?));
        }

        public static int SizeOf<T>(T obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return SizeOf(obj.GetType());
        }

        public static int SizeOf(Type t)
        {
            if (t == null) throw new ArgumentNullException(nameof(t));

            return _cache.GetOrAdd(t, t2 =>
            {
                var dm = new DynamicMethod("$", typeof(int), Type.EmptyTypes);
                ILGenerator il = dm.GetILGenerator();
                il.Emit(OpCodes.Sizeof, t2);
                il.Emit(OpCodes.Ret);

                var func = (Func<int>)dm.CreateDelegate(typeof(Func<int>));
                return func();
            });
        }

        private static readonly ConcurrentDictionary<Type, int>
            _cache = new ConcurrentDictionary<Type, int>();
    }
}
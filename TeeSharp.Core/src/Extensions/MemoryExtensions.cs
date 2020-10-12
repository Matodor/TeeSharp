using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TeeSharp.Core.Extensions
{
    public static class MemoryExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> Deserialize<T>(this Span<byte> buffer, int count) where T : struct
        {
            if (TypeHelper<T>.IsArray == false &&
                TypeHelper<T>.ElementSize * count <= buffer.Length)
            {
                return MemoryMarshal.Cast<byte, T>(buffer);
            }

            throw new ArgumentOutOfRangeException(nameof(buffer));
        }        
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Deserialize<T>(this Span<byte> buffer) where T : struct
        {
            if (TypeHelper<T>.IsArray == false &&
                TypeHelper<T>.Size <= buffer.Length)
            {
                return MemoryMarshal.Read<T>(buffer);
            }

            throw new ArgumentOutOfRangeException(nameof(buffer));
        }
    }
}
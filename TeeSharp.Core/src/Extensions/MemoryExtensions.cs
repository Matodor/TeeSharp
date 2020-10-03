using System;
using System.Runtime.InteropServices;

namespace TeeSharp.Core.Extensions
{
    public static class MemoryExtensions
    {
        public static T ToStruct<T>(this Span<byte> buffer) where T : struct
        {
            return MemoryMarshal.Read<T>(buffer);
        }

        public static Span<T> ToStructs<T>(this Span<byte> buffer) where T : struct
        {
            return MemoryMarshal.Cast<byte, T>(buffer);
        }
    }
}
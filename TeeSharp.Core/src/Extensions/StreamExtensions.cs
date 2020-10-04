using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace TeeSharp.Core.Extensions
{
    public static class StreamExtensions
    {
        public static bool GetStruct<T>(this Stream stream, out T output) where T : struct
        {
            var size = TypeHelper<T>.Size;
            if (stream.Position + size >= stream.Length)
            {
                output = default;
                return false;
            }

            var buffer = new Span<byte>(new byte[size]);
            if (buffer.Length != stream.Read(buffer))
            {
                output = default;
                return false;
            }

            output = buffer.ToStruct<T>();
            return true;
        }

        public static bool GetStructs<T>(this Stream stream, int count, out Span<T> output) where T : struct
        {
            var size = TypeHelper<T>.Size;
            if (stream.Position + size * count >= stream.Length)
            {
                output = default;
                return false;
            }
            
            var buffer = new Span<byte>(new byte[size * count]);
            if (buffer.Length != stream.Read(buffer))
            {
                output = default;
                return false;
            }
            
            output = buffer.ToStructs<T>();
            return true;
        }
    }
}
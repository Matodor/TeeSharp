using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace TeeSharp.Core.Extensions
{
    public static class StreamExtensions
    {
        public static bool Get<T>(this Stream stream, out T output) where T : struct
        {
            if (TypeHelper<T>.IsArray)
                throw new Exception(nameof(T));
            
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

            output = buffer.Deserialize<T>();
            return true;
        }

        public static bool Get<T>(this Stream stream, int count, out Span<T> output) where T : struct
        {
            if (TypeHelper<T>.IsArray)
                throw new Exception(nameof(T));
            
            var size = TypeHelper<T>.ElementSize;
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

            output = buffer.Deserialize<T>(count);
            return true;
        }
    }
}
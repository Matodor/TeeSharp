using System;
using System.IO;
using TeeSharp.Core.Helpers;

namespace TeeSharp.Core.Extensions
{
    public static class StreamExtensions
    {
        public static bool Get<T>(this Stream stream, out T output, int readSize = 0) where T : struct
        {
            if (TypeHelper<T>.IsArray || readSize < 0)
            {
                output = default;
                return false;
            }

            readSize = readSize > 0 
                ? Math.Min(readSize, TypeHelper<T>.Size) 
                : TypeHelper<T>.Size;
            
            if (stream.Position + readSize >= stream.Length)
            {
                output = default;
                return false;
            }

            var buffer = new Span<byte>(new byte[TypeHelper<T>.Size]);
            var readBuffer = readSize != TypeHelper<T>.Size
                ? buffer.Slice(0, readSize)
                : buffer;
            
            if (readBuffer.Length != stream.Read(readBuffer))
            {
                output = default;
                return false;
            }

            output = buffer.Deserialize<T>();
            return true;
        }

        public static bool Get<T>(this Stream stream, int count, out Span<T> output) where T : struct
        {
            var size = TypeHelper<T>.ElementSize;

            if (TypeHelper<T>.IsArray || 
                stream.Position + size * count >= stream.Length)
            {
                output = null;
                return false;
            }
            
            var buffer = new Span<byte>(new byte[size * count]);
            if (buffer.Length != stream.Read(buffer))
            {
                output = null;
                return false;
            }

            output = buffer.Deserialize<T>(count);
            return true;
        }
    }
}
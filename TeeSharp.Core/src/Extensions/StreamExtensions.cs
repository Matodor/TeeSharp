using System;
using System.Buffers;
using System.IO;
using TeeSharp.Core.Helpers;

namespace TeeSharp.Core.Extensions;

public static class StreamExtensions
{
    public static bool TryRead<T>(this Stream stream, out T output) where T : struct
    {
        var bufferSize = StructHelper<T>.Size;
        if (stream.Position + bufferSize >= stream.Length)
        {
            output = default;
            return false;
        }

        var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        if (stream.Read(buffer, 0, bufferSize) != bufferSize)
        {
            output = default;
            return false;
        }

        output = ((ReadOnlySpan<byte>)buffer)
            .Slice(0, StructHelper<T>.Size)
            .Deserialize<T>();

        ArrayPool<byte>.Shared.Return(buffer);
        return true;
    }

    public static bool TryRead<T>(this Stream stream, int count, out T[] output) where T : struct
    {
        var bufferSize = StructHelper<T>.Size * count;
        if (stream.Position + bufferSize >= stream.Length)
        {
            output = Array.Empty<T>();
            return false;
        }

        var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        if (stream.Read(buffer, 0, bufferSize) != bufferSize)
        {
            output = Array.Empty<T>();
            return false;
        }

        output = ((ReadOnlySpan<byte>)buffer)
            .Slice(0, bufferSize)
            .Deserialize<T>(count);

        ArrayPool<byte>.Shared.Return(buffer);
        return true;
    }
}

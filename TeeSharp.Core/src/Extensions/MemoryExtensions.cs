using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using TeeSharp.Core.Helpers;

namespace TeeSharp.Core.Extensions;

public static class MemoryExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Deserialize<T>(this ReadOnlySpan<byte> data, int count) where T : struct
    {
        var items = new T[count];
        for (var i = 0; i < count; i++)
        {
            items[i] = data
                .Slice(StructHelper<T>.Size * i)
                .Deserialize<T>();
        }

        return items;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Deserialize<T>(this ReadOnlySpan<byte> data) where T : struct
    {
        return MemoryMarshal.Read<T>(data);
    }

    public static Span<int> PutString(this Span<int> data, string str)
    {
        if (data.Length == 0)
            return data;

        var buffer = Encoding.UTF8.GetBytes(str);
        var bufferIndex = 0;

        for (var i = 0; i < data.Length; i++)
        {
            var chars = new[] {0, 0, 0, 0};
            for (var charIndex = 0;
                 charIndex < chars.Length && bufferIndex < buffer.Length;
                 charIndex++, bufferIndex++)
            {
                chars[charIndex] = buffer[bufferIndex];
            }

            data[i] =
                ((chars[0] + 128) << 24) |
                ((chars[1] + 128) << 16) |
                ((chars[2] + 128) << 08) |
                ((chars[3] + 128) << 00);
        }

        data[^1] = (int)(data[^1] & 0xFF_FF_FF_00);

        return data;
    }

    public static string GetString(this Span<int> data)
    {
        var buffer = (Span<byte>) stackalloc byte[data.Length * sizeof(int) + 1];
        var length = 0;

        for (var i = 0; i < data.Length; i++)
        {
            buffer[i * 4 + 0] = (byte) (((data[i] >> 24) & 0xFF) - 128);
            if (buffer[i * 4 + 0] < 32)
                break;
            length++;

            buffer[i * 4 + 1] = (byte) (((data[i] >> 16) & 0xFF) - 128);
            if (buffer[i * 4 + 1] < 32)
                break;
            length++;

            buffer[i * 4 + 2] = (byte) (((data[i] >> 8) & 0xFF) - 128);
            if (buffer[i * 4 + 2] < 32)
                break;
            length++;

            buffer[i * 4 + 3] = (byte) ((data[i] & 0xFF) - 128);
            if (buffer[i * 4 + 3] < 32)
                break;
            length++;
        }

        buffer[length] = 0;

        return length == 0
            ? string.Empty
            : Encoding.UTF8.GetString(buffer.Slice(0, length));
    }
}

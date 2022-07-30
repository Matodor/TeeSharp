using System;
using System.Collections.Generic;

namespace TeeSharp.Core;

public static class CompressionableInt
{
    private static readonly IReadOnlyList<int> UnpackShifts = new[]
    {
        6,
        6 + 7,
        6 + 7 + 7,
        6 + 7 + 7 + 7,
    };

    private static readonly IReadOnlyList<int> UnpackMasks = new[]
    {
        0b_0111_1111,
        0b_0111_1111,
        0b_0111_1111,
        0b_0000_1111,
    };

    public static bool TryPack(
        Span<byte> buffer,
        int value,
        ref int bufferIndex)
    {
        if (buffer.IsEmpty)
            return false;

        buffer[bufferIndex] = 0;

        if (value < 0)
        {
            buffer[bufferIndex] |= 0b_0100_0000;
            value = ~value;
        }

        buffer[bufferIndex] |= (byte)(value & 0b_0011_1111);
        value >>= 6;

        while (value != 0)
        {
            if (bufferIndex + 1 == buffer.Length)
                return false;

            buffer[bufferIndex++] |= 0b_1000_0000;
            buffer[bufferIndex] = (byte)(value & 0b_0111_1111);
            value >>= 7;
        }

        bufferIndex++;
        return true;
    }

    public static bool TryUnpack(
        Span<byte> dataIn,
        out int result,
        out Span<byte> dataOut)
    {
        if (dataIn.IsEmpty)
        {
            result = default;
            dataOut = dataIn;
            return false;
        }

        var dataInIndex = 0;
        result = dataIn[0] & 0b_0011_1111;

        for (var i = 0; i < UnpackMasks.Count; i++)
        {
            if ((dataIn[i] & 0b_1000_0000) == 0)
                break;

            if (dataInIndex + 1 >= dataIn.Length)
            {
                result = default;
                dataOut = dataIn;
                return false;
            }

            dataInIndex++;
            result |= (dataIn[dataInIndex] & UnpackMasks[i]) << UnpackShifts[i];
        }

        dataInIndex++;
        result ^= -(dataIn[0] >> 6 & 1);
        dataOut = dataIn.Slice(dataInIndex);
        return true;
    }
}

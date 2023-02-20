using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using Uuids;

namespace TeeSharp.Core.Extensions;

public static class UuidExtensions
{
    private const byte ResetVersionMask  = 0b_0000_1111;
    private const byte Version3Flag      = 0b_0011_0000;

    private const byte ResetReservedMask = 0b_0011_1111;
    private const byte ReservedFlag      = 0b_1000_0000;

    private static readonly Uuid TeeworldsNamespace = Uuid.ParseExact("e05ddaaa-c4e6-4cfb-b642-5d48e80c0029", "d");

    [SuppressMessage("ReSharper", "PossiblyImpureMethodCallOnReadonlyVariable")]
    public static Uuid CalculateUuid(this string str)
    {
        var buffer = (Span<byte>)new byte[16 + Encoding.UTF8.GetMaxByteCount(str.Length)];

        if (!TeeworldsNamespace.TryWriteBytes(buffer.Slice(0, 16)))
            throw new OutOfMemoryException();

        var strBytesCount = Encoding.UTF8.GetBytes(str, buffer.Slice(16));
        var hashData = MD5.HashData(buffer.Slice(0, 16 + strBytesCount));

        // set UUID version 3
        hashData[6] &= ResetVersionMask;
        hashData[6] |= Version3Flag;
        hashData[8] &= ResetReservedMask;
        hashData[8] |= ReservedFlag;

        return new Uuid(hashData);
    }
}

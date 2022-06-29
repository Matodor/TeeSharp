using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using Uuids;

namespace TeeSharp.Common.Protocol;

public static class UuidManager
{
    // public const int UuidOffset = 65536;

    public static class Common
    {
        public static readonly Uuid TeeworldsNamespace = Uuid.Parse("e05ddaaa-c4e6-4cfb-b642-5d48e80c0029");
    }

    [SuppressMessage("ReSharper", "PossiblyImpureMethodCallOnReadonlyVariable")]
    public static Uuid CalculateUuid(string str)
    {
        var buffer = (Span<byte>)new byte[16 + Encoding.UTF8.GetMaxByteCount(str.Length)];

        if (!Common.TeeworldsNamespace.TryWriteBytes(buffer.Slice(0, 16)))
            throw new OutOfMemoryException();

        var strBytesCount = Encoding.UTF8.GetBytes(str, buffer.Slice(16));
        var hashData = MD5.HashData(buffer.Slice(0, 16 + strBytesCount));

        hashData[6] &= 0x0f;
        hashData[6] |= 0x30;
        hashData[8] &= 0x3f;
        hashData[8] |= 0x80;

        return new Uuid(hashData);
    }
}
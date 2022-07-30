using System;
using NUnit.Framework;
using TeeSharp.Core;

namespace TeeSharp.Tests;

public class CompressionTests
{
    [Test]
    public void CompressionableIntTest()
    {
        var buffer = (Span<byte>)new byte[512];
        var bufferIndex = 0;

        Assert.True(CompressionableInt.TryPack(buffer, 1, ref bufferIndex));
        Assert.True(CompressionableInt.TryPack(buffer, 1, ref bufferIndex));
        Assert.True(CompressionableInt.TryPack(buffer, 1, ref bufferIndex));
        Assert.True(CompressionableInt.TryPack(buffer, 123, ref bufferIndex));
        Assert.True(CompressionableInt.TryPack(buffer, -123, ref bufferIndex));
        Assert.True(CompressionableInt.TryPack(buffer, -999, ref bufferIndex));
        Assert.True(CompressionableInt.TryPack(buffer, 999, ref bufferIndex));

        Assert.True(CompressionableInt.TryUnpack(buffer, out var result1, out buffer));
        Assert.True(CompressionableInt.TryUnpack(buffer, out var result2, out buffer));
        Assert.True(CompressionableInt.TryUnpack(buffer, out var result3, out buffer));
        Assert.True(CompressionableInt.TryUnpack(buffer, out var result4, out buffer));
        Assert.True(CompressionableInt.TryUnpack(buffer, out var result5, out buffer));
        Assert.True(CompressionableInt.TryUnpack(buffer, out var result6, out buffer));
        Assert.True(CompressionableInt.TryUnpack(buffer, out var result7, out buffer));

        Assert.AreEqual(result1, 1);
        Assert.AreEqual(result2, 1);
        Assert.AreEqual(result3, 1);
        Assert.AreEqual(result4, 123);
        Assert.AreEqual(result5, -123);
        Assert.AreEqual(result6, -999);
        Assert.AreEqual(result7, 999);
    }
}

using System;
using NUnit.Framework;
using TeeSharp.Core.Extensions;
using TeeSharp.Map.DataFileItems;

namespace TeeSharp.Tests;

public class MapTests
{
    [Test]
    public void DeserializeDataFileHeaderTest()
    {
        // map header for "Kobra 4"
        var data = (ReadOnlySpan<byte>) new byte[]
        {
            68, 65, 84, 65, 4, 0, 0, 0, 215, 93, 4, 0, 212, 11, 0, 0, 6, 0, 0, 0, 34, 0, 0, 0, 41, 0, 0, 0, 168, 9,
            0, 0, 3, 82, 4, 0,
        };

        var header = data.Deserialize<DataFileHeader>();

        Assert.True(header.IsValidSignature);
        Assert.True(header.IsValidVersion);

        Assert.AreEqual(header.RawDataBlocksSize, 283139);
        Assert.AreEqual(header.ItemsSize, 2472);
        Assert.AreEqual(header.NumberOfRawDataBlocks, 41);
        Assert.AreEqual(header.NumberOfItemTypes, 6);
        Assert.AreEqual(header.NumberOfItems, 34);
        Assert.AreEqual(header.Size, 286167);
        Assert.AreEqual(header.SwapLength, 3028);
        Assert.AreEqual(header.Version, 4);
    }

    [Test]
    public void DeserializeMultipleDataFileHeaderTest()
    {
        const int count = 10;

        var data = (ReadOnlySpan<byte>) new byte[]
        {
            68, 65, 84, 65, 4, 0, 0, 0, 215, 93, 4, 0, 212, 11, 0, 0, 6, 0, 0, 0, 34, 0, 0, 0, 41, 0, 0, 0, 168, 9,
            0, 0, 3, 82, 4, 0,
        };

        var buffer = (Span<byte>)new byte[data.Length * count];
        for (var i = 0; i < count; i++)
            data.CopyTo(buffer.Slice(i * data.Length));

        var headers = ((ReadOnlySpan<byte>)buffer).Deserialize<DataFileHeader>(count);
        Assert.AreEqual(headers.Length, count);

        foreach (var header in headers)
        {
            Assert.True(header.IsValidSignature);
            Assert.True(header.IsValidVersion);

            Assert.AreEqual(header.RawDataBlocksSize, 283139);
            Assert.AreEqual(header.ItemsSize, 2472);
            Assert.AreEqual(header.NumberOfRawDataBlocks, 41);
            Assert.AreEqual(header.NumberOfItemTypes, 6);
            Assert.AreEqual(header.NumberOfItems, 34);
            Assert.AreEqual(header.Size, 286167);
            Assert.AreEqual(header.SwapLength, 3028);
            Assert.AreEqual(header.Version, 4);
        }
    }
}

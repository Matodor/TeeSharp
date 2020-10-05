using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NUnit.Framework;
using TeeSharp.Core.Extensions;
using TeeSharp.Map;

namespace TeeSharp.Tests
{
    public class DataFileTests
    {
        [Test]
        public void DeserializeDatafileHeaderTest()
        {
            var data = new byte[]
            {
                68, 65, 84, 65, 4, 0, 0, 0, 215, 93, 4, 0, 212, 11, 0, 0, 6, 0, 0, 0, 34, 0, 0, 0, 41, 0, 0, 0, 168, 9,
                0, 0, 3, 82, 4, 0, 0
            };

            var header = data.AsSpan().ToStruct<DataFileHeader>();
            Assert.True(header.IsValidSignature);
            Assert.True(header.IsValidVersion);
            
            Assert.AreEqual(header.DataSize, 283139);
            Assert.AreEqual(header.ItemSize, 2472);
            Assert.AreEqual(header.RawDataBlocks, 41);
            Assert.AreEqual(header.ItemTypesCount, 6);
            Assert.AreEqual(header.ItemsCount, 34);
            Assert.AreEqual(header.Size, 286167);
            Assert.AreEqual(header.SwapLength, 3028);
            Assert.AreEqual(header.Version, 4);
        }
    }
}
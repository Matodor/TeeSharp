using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
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
            // map header for "Kobra 4"
            var data = (Span<byte>) new byte[]
            {
                68, 65, 84, 65, 4, 0, 0, 0, 215, 93, 4, 0, 212, 11, 0, 0, 6, 0, 0, 0, 34, 0, 0, 0, 41, 0, 0, 0, 168, 9,
                0, 0, 3, 82, 4, 0
            };

            var header = data.Deserialize<DataFileHeader>();
                
            Assert.True(header.IsValidSignature);
            Assert.True(header.IsValidVersion);
            
            Assert.AreEqual(header.RawDataBlocksSize, 283139);
            Assert.AreEqual(header.ItemsSize, 2472);
            Assert.AreEqual(header.RawDataBlocks, 41);
            Assert.AreEqual(header.ItemTypesCount, 6);
            Assert.AreEqual(header.ItemsCount, 34);
            Assert.AreEqual(header.Size, 286167);
            Assert.AreEqual(header.SwapLength, 3028);
            Assert.AreEqual(header.Version, 4);
        }
    }
}
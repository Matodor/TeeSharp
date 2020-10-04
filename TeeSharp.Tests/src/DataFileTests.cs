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
                68, 65, 84, 65, 4, 0, 0, 0, 156, 125, 3, 0, 252, 12, 0, 0, 7, 0, 0, 0, 39, 0, 0, 0, 36, 0, 0, 0, 216,
                10, 0, 0, 160, 112, 3, 0, 0
            };

            var header = data.AsSpan().ToStruct<DataFileHeader>();
            Assert.True(header.IsValidSignature);
            Assert.True(header.IsValidVersion);
        }
    }
}
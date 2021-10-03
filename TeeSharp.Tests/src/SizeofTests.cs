using System;
using System.Linq;
using NUnit.Framework;
using TeeSharp.Core.Helpers;
using TeeSharp.Map;
using TeeSharp.Network;

namespace TeeSharp.Tests
{
    public class SizeofTests
    {
        [Test]
        public void CheckSize()
        {
            Assert.AreEqual(36, StructHelper<DataFileHeader>.Size);
            Assert.AreEqual(4, StructHelper<SecurityToken>.Size);
        }
    }
}
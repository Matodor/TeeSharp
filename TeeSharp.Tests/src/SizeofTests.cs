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
            Assert.AreEqual(36, TypeHelper<DataFileHeader>.Size);
            Assert.AreEqual(4, TypeHelper<SecurityToken>.Size);
        }
    }
}
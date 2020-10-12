using NUnit.Framework;
using TeeSharp.Core;
using TeeSharp.Map;
using TeeSharp.MasterServer;

namespace TeeSharp.Tests
{
    public class SizeofTests
    {
        [Test]
        public void CheckSize()
        {
            Assert.AreEqual(36, TypeHelper<DataFileHeader>.Size);
        }
    }
}
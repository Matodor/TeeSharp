using NUnit.Framework;
using TeeSharp.Core.Helpers;
using TeeSharp.Map;

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
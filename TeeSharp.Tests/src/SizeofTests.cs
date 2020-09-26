using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NUnit.Framework;
using TeeSharp.MasterServer;

namespace TeeSharp.Tests
{
    public class SizeofTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            Assert.AreEqual(18, Marshal.SizeOf<ServerEndpoint>());
        }
    }
}
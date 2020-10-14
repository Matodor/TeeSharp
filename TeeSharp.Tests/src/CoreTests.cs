using System;
using NUnit.Framework;
using TeeSharp.Core;
using TeeSharp.Core.Extensions;
using TeeSharp.Map;
using TeeSharp.MasterServer;

namespace TeeSharp.Tests
{
    public class CoreTests
    {
        [Test]
        public void StrToInts()
        {
            var ints1 = new Span<int>(new int[3]).PutString("Matodor");
            var ints2 = new Span<int>(new int[3] { -840829713, -454036864, -2139062272 });
            
            Assert.True(ints1.SequenceEqual(ints2));
        }        
        
        [Test]
        public void IntsToStr()
        {
            var ints = new int[3] {-840829713, -454036864, -2139062272}.AsSpan();
            Assert.AreEqual("Matodor", ints.GetString());
        }
    }
}
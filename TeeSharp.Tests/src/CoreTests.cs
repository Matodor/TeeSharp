using System;
using System.Diagnostics;
using NUnit.Framework;
using TeeSharp.Core.Extensions;
using TeeSharp.Core.Helpers;

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

        [Test]
        public void ShouldThreadSleeps()
        {
            const int millis = 5000;
            
            // 5ms error
            var error = 5;
            
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var beforeMilliseconds = stopWatch.ElapsedMilliseconds;
                
            ThreadsHelper.SleepForNoMoreThan(millis);
            
            if (stopWatch.ElapsedMilliseconds <= beforeMilliseconds + millis + error)
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}
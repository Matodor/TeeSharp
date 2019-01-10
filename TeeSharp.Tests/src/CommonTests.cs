using Microsoft.VisualStudio.TestTools.UnitTesting;
using TeeSharp.Core;
using TeeSharp.Core.Extensions;

namespace TeeSharp.Tests
{
    [TestClass]
    public class CommonTests
    {
        [TestMethod]
        public void TestIntsToStr()
        {
            const int arraySize = 4;
            var actual = new int[arraySize]
            {
                -723130889, -269292316, -204868660, -269032192
            }.IntsToStr();
            Assert.IsTrue(actual == "TeeworldsIsLoveeeeee".Limit(sizeof(int) * arraySize - 1)); // Max string length (sizeof(int) * 4 - 1
        }

        [TestMethod]
        public void TestStrToInt()
        {
            var actual = "wL7SHc4Ipa1prqHE".StrToInts(4);
            var expected = new int[]
            {
                -137578541, -924601143, -253644304, -219035648
            };

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
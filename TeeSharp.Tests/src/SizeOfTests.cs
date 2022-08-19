using NUnit.Framework;
using TeeSharp.Core.Helpers;
using TeeSharp.Map.DataFileItems;
using TeeSharp.Network;

namespace TeeSharp.Tests;

public class SizeOfTests
{
    [Test]
    public void CheckSize()
    {
        Assert.AreEqual(36, StructHelper<DataFileHeader>.Size);
        Assert.AreEqual(4, StructHelper<SecurityToken>.Size);
    }
}

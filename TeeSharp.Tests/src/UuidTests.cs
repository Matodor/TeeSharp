using NUnit.Framework;
using TeeSharp.Core.Extensions;
using Uuids;

namespace TeeSharp.Tests;

public class UuidTests
{
    [Test]
    public void TestCreateFromArray()
    {
        foreach (var (uuid, uuidData) in new[]
            {
                (
                    // TEEWORLDS_NAMESPACE
                    "e05ddaaa-c4e6-4cfb-b642-5d48e80c0029",
                    new byte[]
                    {
                        0xe0, 0x5d, 0xda, 0xaa, 0xc4, 0xe6, 0x4c, 0xfb,
                        0xb6, 0x42, 0x5d, 0x48, 0xe8, 0x0c, 0x00, 0x29,
                    }
                ),
                (
                    // NETMSG_CLIENTVER: "clientver@ddnet.tw"
                    "8c001304-8461-3e47-8787-f672b3835bd4",
                    new byte[]
                    {
                        140, 0, 19, 4, 132, 97, 62, 71,
                        135, 135, 246, 114, 179, 131, 91, 212,
                    }
                ),
            }
        )
        {
            var uuidFromStr = Uuid.Parse(uuid);
            var uuidFromBytes = new Uuid(uuidData);

            Assert.AreEqual(uuidFromStr, uuidFromBytes);
        }
    }

    [Test]
    public void TestCalculate()
    {
        var uuidCalculated = "clientver@ddnet.tw".CalculateUuid();
        var uuid = Uuid.Parse("8c001304-8461-3e47-8787-f672b3835bd4");

        Assert.AreEqual(uuidCalculated, uuid);
    }

    [Test]
    public void TestFormatUuid()
    {
        var expected = "8c001304-8461-3e47-8787-f672b3835bd4";
        var formatted = Uuid.Parse(expected).ToString("d");

        Assert.AreEqual(expected, formatted);
    }
}

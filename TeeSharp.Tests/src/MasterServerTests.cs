using System.Net;
using NUnit.Framework;
using TeeSharp.MasterServer;

namespace TeeSharp.Tests;

public class MasterServerTests
{
    [Test]
    public void DeserializeServerEndpointTest()
    {
        var data = new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111};
        var endPoint1 = MasterServerHelper.DeserializeEndPoint(data);
        var endPoint2 = new IPEndPoint(IPAddress.Parse("192.168.0.123"), 8303);

        Assert.AreEqual(endPoint1, endPoint2);
    }

    [Test]
    public void DeserializeMultipleServerEndpointTest()
    {
        var data = new byte[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111,
        };

        var endPoints = MasterServerHelper.EndPointDeserializeMultiple(data);
        var endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.123"), 8303);

        Assert.AreEqual(endPoint, endPoints[0]);
        Assert.AreEqual(endPoint, endPoints[1]);
        Assert.AreEqual(endPoint, endPoints[2]);
    }

    [Test]
    public void SerializeEndpointTest()
    {
        var endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.123"), 8303);
        var buffer1 = MasterServerHelper.SerializeEndPoint(endPoint).ToArray();
        var buffer2 = new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111};

        CollectionAssert.AreEqual(buffer1, buffer2);
    }



    [Test]
    public void ShouldEqualsServerInfo()
    {
        var info1 = new ServerInfo
        {
            Name = "TeeSharp - test MasterServerInteractor",
            GameType = "TeeSharp",
            HasPassword = true,
            Version = "0.6.4",
            MaxPlayers = 32,
            MaxClients = 32,
            Map = new ServerInfoMap
            {
                Name = "test",
                Size = 128,
                Checksum = "test",
            },
            Clients = new ServerInfoClient[]
            {
                new()
                {
                    Name = "Matodor",
                    IsAfk = true,
                    Team = -1,
                    Clan = "test",
                    Country = 0,
                    IsPlayer = true,
                    Score = 666,
                    Skin = new ServerInfoClientSkin
                    {
                        Name = "pinky",
                        ColorBody = 0,
                        ColorFeet = 0,
                    },
                },
            },
        };


        var info2 = new ServerInfo
        {
            Name = "TeeSharp - test MasterServerInteractor",
            GameType = "TeeSharp",
            HasPassword = true,
            Version = "0.6.4",
            MaxPlayers = 32,
            MaxClients = 32,
            Map = new ServerInfoMap
            {
                Name = "test",
                Size = 128,
                Checksum = "test",
            },
            Clients = new ServerInfoClient[]
            {
                new()
                {
                    Name = "Matodor",
                    IsAfk = true,
                    Team = -1,
                    Clan = "test",
                    Country = 0,
                    IsPlayer = true,
                    Score = 666,
                    Skin = new ServerInfoClientSkin
                    {
                        Name = "pinky",
                        ColorBody = 0,
                        ColorFeet = 0,
                    },
                },
            },
        };

        Assert.AreEqual(info1, info2);
    }
}

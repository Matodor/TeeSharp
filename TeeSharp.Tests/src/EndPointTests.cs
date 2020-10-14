using System.Net;
using NUnit.Framework;
using TeeSharp.MasterServer;

namespace TeeSharp.Tests
{
    public class EndPointTests
    {
        [Test]
        public void DeserializeServerEndpointTest()
        {
            var data = new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 192, 168, 0, 123, 32, 111};
            var endPoint1 = ServerEndpoint.Get(data);
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
            
            var endPoints = ServerEndpoint.GetArray(data);
            var endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.123"), 8303);
            
            Assert.AreEqual(endPoint, endPoints[0]);
            Assert.AreEqual(endPoint, endPoints[1]);
            Assert.AreEqual(endPoint, endPoints[2]);
        }
    }
}
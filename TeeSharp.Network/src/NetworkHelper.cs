using System.Net;
using System.Net.Sockets;

namespace TeeSharp.Network
{
    public static class NetworkHelper
    {
        public static bool CreateUdpClient(IPEndPoint endPoint, out UdpClient client)
        {
            try
            {
                client = new UdpClient(endPoint) { Client = { Blocking = false } };
                return true;
            }
            catch
            {
                client = null;
                return false;
            }
        }
    }
}
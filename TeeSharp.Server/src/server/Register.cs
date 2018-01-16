using System.Net.Sockets;

namespace TeeSharp.Server
{
    public class Register : BaseRegister
    {
        public override void RegisterUpdate(AddressFamily netType)
        {
            // ipv4 type == AddressFamily.InterNetwork
            // ipv6 type == AddressFamily.InterNetworkV6
        }
    }
}
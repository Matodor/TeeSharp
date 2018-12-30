using System.Net.Sockets;
using TeeSharp.Network;

namespace TeeSharp.Server
{
    public class Register : BaseRegister
    {
        public override void RegisterUpdate(AddressFamily netType)
        {
            // ipv4 type == AddressFamily.InterNetwork
            // ipv6 type == AddressFamily.InterNetworkV6
        }

        public override bool RegisterProcessPacket(Chunk packet, uint token)
        {
            return false;
        }
    }
}
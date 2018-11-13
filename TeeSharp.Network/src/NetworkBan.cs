using System.Net;

namespace TeeSharp.Network
{
    public class NetworkBan : BaseNetworkBan
    {
        public override void Update()
        {
        }

        public override bool BanAddr(IPEndPoint clientAddr, int seconds, string reason)
        {
            return true;
        }

        public override bool IsBanned(IPEndPoint remote, out string reason)
        {
            reason = null;
            return false;
        }
    }
}
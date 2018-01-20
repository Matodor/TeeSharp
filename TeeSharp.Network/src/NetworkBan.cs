using System.Net;

namespace TeeSharp.Network
{
    public class NetworkBan : BaseNetworkBan
    {
        public override void Update()
        {
        }

        public override void BanAddr(IPEndPoint clientAddr, int seconds, string reason)
        {
            throw new System.NotImplementedException();
        }

        public override bool IsBanned(IPEndPoint remote, out string reason)
        {
            throw new System.NotImplementedException();
        }
    }
}
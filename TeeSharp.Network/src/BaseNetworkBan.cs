using System.Net;
using TeeSharp.Core;

namespace TeeSharp.Network
{
    public abstract class BaseNetworkBan : BaseInterface
    {
        public abstract void Update();
        public abstract void BanAddr(IPEndPoint clientAddr, int seconds, string reason);
        public abstract bool IsBanned(IPEndPoint remote, out string reason);
    }
}
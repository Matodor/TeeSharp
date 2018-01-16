using System.Net;
using TeeSharp.Common;

namespace TeeSharp.Network
{
    public abstract class BaseServerBan : BaseInterface
    {
        public abstract void Update();
        public abstract void BanAddr(IPEndPoint clientAddr, int seconds, string reason);
    }
}
using System.Net.Sockets;

namespace TeeSharp.Network
{
    public abstract class BaseNetworkServer
    {
        protected UdpClient Socket { get; set; }
        
        public abstract void Init();
        public abstract void Update();
        public abstract bool Open();
    }
}
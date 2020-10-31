using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace TeeSharp.Network
{
    public abstract class BaseNetworkServer
    {
        protected UdpClient Socket { get; set; }
        
        public abstract void Init();
        
        public abstract void Update();
        
        // ReSharper disable once InconsistentNaming
        public abstract bool Open(IPEndPoint localEP);

        public abstract bool Receive();
    }
}
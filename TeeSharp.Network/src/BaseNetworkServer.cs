using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using TeeSharp.Core.MinIoC;

namespace TeeSharp.Network
{
    public abstract class BaseNetworkServer : IContainerService
    {
        public Container Container { get; set; }
        
        protected UdpClient Socket { get; set; }

        public abstract void Init();
        
        public abstract void Update();
        
        // ReSharper disable once InconsistentNaming
        public abstract bool Open(IPEndPoint localEP);

        public abstract bool Receive();
    }
}
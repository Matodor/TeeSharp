using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace TeeSharp.Network
{
    public class NetworkServer : BaseNetworkServer
    {
        public override void Init()
        {
            
        }

        public override void Update()
        {
            
        }

        // ReSharper disable once InconsistentNaming
        public override bool Open(IPEndPoint localEP)
        {
            if (localEP == null)
                throw new ArgumentNullException(nameof(localEP));
            
            if (!NetworkBase.TryGetUdpClient(localEP, out var socket))
                return false;

            Socket = socket;
            Socket.Client.Blocking = true;
            
            return true;
        }

        public override bool Receive()
        {
            while (true)
            {
                // ReSharper disable once InconsistentNaming
                var remoteEP = default(IPEndPoint);
                var data = Socket.Receive(ref remoteEP).AsSpan();
                
                if (data.Length == 0) 
                    continue;

                var chunks = new NetworkChunks();
                var isSixUp = false;
                var securityToken = default(SecurityToken);
                var responseToken = default(SecurityToken);

                if (!NetworkBase.TryUnpackPacket(
                    data, 
                    chunks,
                    ref isSixUp,
                    ref securityToken,
                    ref responseToken))
                {
                    continue;
                }
                
                
            }

            return true;
        }
    }
}
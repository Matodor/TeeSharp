using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TeeSharp.Core;
using TeeSharp.MasterServer;
using TeeSharp.Network;

namespace Examples.MasterServer
{
    internal class KernelConfig : IKernelConfig
    {
        public void Load(IKernel kernel)
        {
            kernel.Bind<BaseNetworkConnection>().To<NetworkConnection>();
            kernel.Bind<BaseChunkReceiver>().To<ChunkReceiver>();
            kernel.Bind<BaseNetworkClient>().To<NetworkClient>().AsSingleton();
        }
    }

    internal class Program
    {
        private static MasterServerBrowser _masterServerBrowser;
        private static BaseNetworkClient _networkClient;

        static void Main(string[] args)
        {
            var kernel = new Kernel(new KernelConfig());

            _networkClient = kernel.Get<BaseNetworkClient>();
            _networkClient.Init();
            _networkClient.Open(new NetworkClientConfig
            {
                LocalEndPoint = new IPEndPoint(IPAddress.Any, 0),
            });

            _masterServerBrowser = new MasterServerBrowser(_networkClient, new[]
            {
                new IPEndPoint(GetIP("master1.teeworlds.com"), 8300),
                new IPEndPoint(GetIP("master2.teeworlds.com"), 8300),
                new IPEndPoint(GetIP("master3.teeworlds.com"), 8300),
                new IPEndPoint(GetIP("master4.teeworlds.com"), 8300),
            });

            _masterServerBrowser.RequestServers();

            while (true)
            {
                while (_networkClient.Receive(out var packet))
                {
                    if (packet.ClientId == -1)
                        _masterServerBrowser.OnPacket(packet);
                }

                _masterServerBrowser.Tick();
                Thread.Sleep(5);
            }
        }

        private static IPAddress GetIP(string hostname)
        {
            var hostEntry = Dns.GetHostEntry(hostname);
            return hostEntry.AddressList.Length > 0 ? hostEntry.AddressList[0] : null;
        }
    }
}
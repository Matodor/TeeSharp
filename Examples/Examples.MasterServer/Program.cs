using System;
using System.Net;
using System.Threading;
using TeeSharp.Core;
using TeeSharp.MasterServer;
using TeeSharp.Network;
using TeeSharp.Network.Enums;

namespace Examples.MasterServer
{
    internal class KernelConfig : IKernelConfig
    {
        public void Load(IKernel kernel)
        {
            kernel.Bind<BaseNetworkConnection>().To<NetworkConnection>();
            kernel.Bind<BaseChunkReceiver>().To<ChunkReceiver>();
            kernel.Bind<BaseTokenManager>().To<TokenManager>();
            kernel.Bind<BaseTokenCache>().To<TokenCache>();
            kernel.Bind<BaseNetworkClient>().To<NetworkClient>().AsSingleton();
        }
    }

    internal class Program
    {
        private static MasterServerBrowser _masterServerBrowser;
        private static BaseNetworkClient _networkClient;
        private static Thread _consoleReader;
        private static bool _isRunning;

        static void Main(string[] args)
        {
            var kernel = new Kernel(new KernelConfig());

            _isRunning = true;
            _consoleReader = new Thread(ConsoleRead);
            _consoleReader.Start();

            _networkClient = kernel.Get<BaseNetworkClient>();
            _networkClient.Init();
            _networkClient.Open(new NetworkClientConfig
            {
                LocalEndPoint = new IPEndPoint(IPAddress.Any, 0),
            });

            SendGetInfo();

            Chunk packet = null;
            uint token = 0;
            while (_isRunning)
            {
                while (_networkClient.Receive(ref packet, ref token))
                {
                    {

                    }
                }

                Thread.Sleep(5);
            }

            //_masterServerBrowser = new MasterServerBrowser(_networkClient, new[]
            //{
            //    new IPEndPoint(GetIP("master1.teeworlds.com"), 8300),
            //    new IPEndPoint(GetIP("master2.teeworlds.com"), 8300),
            //    new IPEndPoint(GetIP("master3.teeworlds.com"), 8300),
            //    new IPEndPoint(GetIP("master4.teeworlds.com"), 8300),
            //});

            //_masterServerBrowser.RequestServers();

            //Chunk packet = null;
            //uint token = 0;

            //while (true)
            //{
            //    while (_networkClient.Receive(ref packet, ref token))
            //    {
            //        if (packet.ClientId == -1)
            //            _masterServerBrowser.OnPacket(packet);
            //    }

            //    _masterServerBrowser.Tick();
            //    Thread.Sleep(5);
            //}
        }

        private static void SendGetInfo()
        {
            var packer = new Packer();
            packer.Reset();
            packer.AddRaw(MasterServerPackets.GetInfo);
            packer.AddInt(RNG.Int());

            var packet = new Chunk();
            packet.EndPoint = new IPEndPoint(IPAddress.Broadcast, 8303);
            packet.ClientId = -1;
            packet.Flags = SendFlags.Connless;
            packet.DataSize = packer.Size();
            packet.Data = packer.Data();

            _networkClient.Send(packet);
        }

        private static void ConsoleRead()
        {
            while (_isRunning)
            {
                var line = Console.ReadLine();

                if (line == "exit")
                    _isRunning = false;
                else if (line == "do")
                    SendGetInfo();
            }
        }

        private static IPAddress GetIP(string hostname)
        {
            var hostEntry = Dns.GetHostEntry(hostname);
            return hostEntry.AddressList.Length > 0 ? hostEntry.AddressList[0] : null;
        }
    }
}
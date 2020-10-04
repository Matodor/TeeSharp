using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TeeSharp.MasterServer;
using TeeSharp.Network;

namespace Examples.ServerBrowser
{
    class Program
    {
        static void Main(string[] args)
        {
            for (var i = 1; i <= 4; i++)
            {
                var masterServerPort = 8300;
                var masterServerAddr = $"master{i}.teeworlds.com";

                var addresses = Dns.GetHostAddresses(masterServerAddr);
                if (addresses.Length == 0)
                {
                    Console.WriteLine("Cant resolve master server address!");
                    return;
                }

                var masterServerEndpoint = new IPEndPoint(addresses[0], masterServerPort);
                var client = new UdpClient
                {
                    Client =
                    {
                        Blocking = false
                    }
                };

                var packet = new Packet()
                {
                    ClientId = -1,
                    Flags = PacketFlags.Connless,
                    Data = Packets.GetList,
                    EndPoint = masterServerEndpoint
                };

                NetworkBase.SendData(client, packet.EndPoint, packet.Data);

                Thread.Sleep(2300);

                if (client.Available > 0)
                {
                    var remote = default(IPEndPoint);
                    var data = client.Receive(ref remote);
                    var test = string.Join(',', data.Select(b => $"{b}"));
                }
            }
            
            Console.WriteLine("Hello World!");
        }
    }
}

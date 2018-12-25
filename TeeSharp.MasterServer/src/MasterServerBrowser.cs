using System;
using System.Net;
using TeeSharp.Core;
using TeeSharp.Network;
using TeeSharp.Network.Enums;
using TeeSharp.Network.Extensions;

namespace TeeSharp.MasterServer
{
    public class MasterServerBrowser
    {
        public event Action<IPEndPoint> OnServerAddrReceived = delegate { };
        public ServersSnapshot ServersSnapshot { get; private set; }

        private readonly BaseNetworkClient _networkClient;
        private readonly IPEndPoint[] _masterServers;
        
        private readonly byte[] _IPV4Mapping = new byte[]
        {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF
        };

        public MasterServerBrowser(BaseNetworkClient netClient, IPEndPoint[] masterServers)
        {
            ServersSnapshot = new ServersSnapshot();
            _masterServers = masterServers;
            _networkClient = netClient;
        }

        public void Tick()
        {
            
        }

        public void OnPacket(Chunk packet)
        {
            // get servers from masterserver
            if (packet.DataSize >= MasterServerPackets.List.Length + 1 &&
                packet.Data.ArrayCompare(
                    MasterServerPackets.List,
                    MasterServerPackets.List.Length))
            {
                if (!IsMasterServer(packet.EndPoint))
                    return;

                var list = packet.Data.ReadStructs<MasterServerAddr>(
                    MasterServerPackets.List.Length);

                for (var i = 0; i < list.Length; i++)
                {
                    IPAddress ip;
                    var port = (list[i].Port[0] << 8) | list[i].Port[1];

                    if (_IPV4Mapping.ArrayCompare(list[i].Ip, _IPV4Mapping.Length))
                    {
                        ip = new IPAddress(new[]
                        {
                            list[i].Ip[12],
                            list[i].Ip[13],
                            list[i].Ip[14],
                            list[i].Ip[15],
                        });
                    }
                    else
                    {
                        ip = new IPAddress(list[i].Ip);
                    }

                    var serverAddr = new IPEndPoint(ip, port);
                    ServersSnapshot.AddServer(serverAddr);
                    OnServerAddrReceived(serverAddr);
                }
            }
        }

        private bool IsMasterServer(IPEndPoint endPoint)
        {
            for (var i = 0; i < _masterServers.Length; i++)
            {
                if (_masterServers[i].Compare(endPoint, true))
                {
                    return true;
                }
            }

            return false;
        }

        public void RequestServers()
        {
            for (var i = 0; i < _masterServers.Length; i++)
            {
                RequestServers(_masterServers[i]);
            }
        }

        private void RequestServers(IPEndPoint masterServer)
        {
            Debug.Log("masterserver", $"request servers list from {masterServer}");

            _networkClient.Send(new Chunk
            {
                ClientId = -1,
                Flags = SendFlags.Connless,
                DataSize = MasterServerPackets.GetList.Length,
                Data = MasterServerPackets.GetList,
                EndPoint = masterServer
            });

            /*Thread.Sleep(TIMEOUT);
            var addr = new IPEndPoint(IPAddress.Any, 8300);

            while (true)
            {
                if (client.Available > 0)
                {
                    byte[] data = null;
                    try
                    {
                        data = client.Receive(ref addr);
                    }
                    catch
                    {
                        // 
                    }

                    if (data == null)
                        break;

                    for (int i = 14; i + 18 <= data.Length; i += 18)
                    {
                        var str = Encoding.ASCII.GetString(data, i, 12);
                        string ip;

                        if (str == "\0\0\0\0\0\0\0\0\0\0??")
                            ip = data[i + 12] + "." + data[i + 13] + "." + data[i + 14] + "." + data[i + 15];
                        else
                            ip = data[i + 12] + "." + data[i + 13] + "." + data[i + 14] + "." + data[i + 15];

                        int t = data[i + 16] << 8;
                        int port = t + data[i + 17];

                        serverList.Add(ip + ":" + port);
                    }
                }
                else
                    break;
            }*/
        }
    }
}
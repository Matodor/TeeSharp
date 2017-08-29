using System;
using System.Net;
using System.Net.Sockets;

namespace TeeSharp
{
    public class NetworkServer : INetworkServer
    {
        protected NetworkBan _netBan;
        protected Configuration _config;
        protected UdpClient _udpClient;

        protected int _maxClientsPerIp;

        protected NewClientCallback _newClientCallback;
        protected DelClientCallback _delClientCallback;

        protected Slot[] _networkSlots;
        protected NetworkReceiveUnpacker _recvUnpacker;

        public NetworkServer()
        {
            
        }

        public virtual void Init()
        {
            _netBan = Kernel.Get<NetworkBan>();
            _config = Kernel.Get<Configuration>();
            _recvUnpacker = new NetworkReceiveUnpacker();
        }

        public virtual void Open(IPEndPoint endPoint, int maxClients, int maxClientsPerIp)
        {
            if (!Base.CreateUdpClient(endPoint, out _udpClient))
                throw new Exception($"couldn't open socket. port {_config.GetInt("SvPort")} might already be in use");

            _networkSlots = new Slot[Consts.NET_MAX_CLIENTS];
            SetMaxClientsPerIp(maxClientsPerIp);

            for (int i = 0; i < _networkSlots.Length; i++)
                _networkSlots[i] = new Slot();
        }

        public void SetCallbacks(NewClientCallback newClientCallback, DelClientCallback delClientCallback)
        {
            _newClientCallback = newClientCallback;
            _delClientCallback = delClientCallback;
        }

        public void Drop(int clientId, string reason)
        {
            _delClientCallback?.Invoke(clientId, reason);
            _networkSlots[clientId].Connection.Disconnect(reason);
        }

        public void SetMaxClientsPerIp(int maxClients)
        {
            _maxClientsPerIp = Math.Clamp(maxClients, 1, Consts.NET_MAX_CLIENTS);
        }

        public void Update()
        {
            var time = Base.TimeGet();
            for (int i = 0; i < Consts.NET_MAX_CLIENTS; i++)
            {
                _networkSlots[i].Connection.Update();
                if (_networkSlots[i].Connection.ConnectionState == ConnectionState.ERROR)
                {
                    //if (Now - m_aSlots[i].m_Connection.ConnectTime() < time_freq() && NetBan())
                    //    NetBan()->BanAddr(ClientAddr(i), 60, "Stressing network");
                    //else
                        Drop(i, _networkSlots[i].Connection.ErrorString());
                }
            }
        }

        public bool Receive(out NetChunk packet)
        {
            while (_udpClient.Available > 0)
            {
                // check for a chunk
                if (_recvUnpacker.FetchChunk(out packet))
                    return true;

                var remote = new IPEndPoint(IPAddress.Any, 0);
                var data = Base.ReceiveUdp(_udpClient, ref remote);

                // check if we just should drop the packet
                string banReason;
                if (_netBan.IsBanned(remote, out banReason))
                {
                    // banned, reply with a message
                    NetworkBase.SendControlMsg(_udpClient, remote, 0, ControlMessage.CLOSE, banReason);
                    continue;
                }

                if (NetworkBase.UnpackPacket(data, data.Length, _recvUnpacker.PacketConstruct))
                {
                    if ((_recvUnpacker.PacketConstruct.Flags & PacketFlag.CONNLESS) != 0)
                    {
                        packet = new NetChunk()
                        {
                            Flags = SendFlag.CONNLESS,
                            ClientId = -1,
                            Address = remote,
                            DataSize = _recvUnpacker.PacketConstruct.DataSize,
                            Data = _recvUnpacker.PacketConstruct.ChunkData
                        };
                        return true;
                    }

                    // drop invalid ctrl packets
                    if ((_recvUnpacker.PacketConstruct.Flags & PacketFlag.CONTROL) != 0 &&
                         _recvUnpacker.PacketConstruct.DataSize == 0)
                        continue;

                    int slotId;
                    var slot = FindSlot(remote, true, out slotId);

                    if ((_recvUnpacker.PacketConstruct.Flags & PacketFlag.CONTROL) != 0 &&
                        (ControlMessage) _recvUnpacker.PacketConstruct.ChunkData[0] == ControlMessage.CONNECT)
                    {
                        if (slot == null)
                        {
                            // allow only a specific number of players with the same ip
                            var sameIps = 0;
                            for (int i = 0; i < Consts.NET_MAX_CLIENTS; i++)
                            {
                                if (_networkSlots[i].Connection.ConnectionState == ConnectionState.OFFLINE)
                                    continue;

                                if (_networkSlots[i].Connection.PeerAddr.Address.Equals(remote.Address))
                                {
                                    sameIps++;
                                    if (sameIps >= _maxClientsPerIp)
                                    {
                                        NetworkBase.SendControlMsg(_udpClient, remote, 0, ControlMessage.CLOSE,
                                            $"Only {_maxClientsPerIp} players with the same IP are allowed");
                                        return false;
                                    }
                                }

                            }

                            // find free slot
                            for (int i = 0; i < Consts.NET_MAX_CLIENTS; i++)
                            {
                                if (_networkSlots[i].Connection.ConnectionState == ConnectionState.OFFLINE)
                                {
                                    _networkSlots[i].Connection.Feed(_recvUnpacker.PacketConstruct, remote);
                                    NewClient(i);
                                    return false;
                                }
                            }

                            NetworkBase.SendControlMsg(_udpClient, remote, 0, ControlMessage.CLOSE, 
                                "This server is full");
                            return false;
                        }
                    }
                    else
                    {
                        // normal packet
                        if (slot != null)
                        {
                            if (slot.Connection.Feed(_recvUnpacker.PacketConstruct, remote))
                            {
                                if (_recvUnpacker.PacketConstruct.DataSize > 0)
                                    _recvUnpacker.Start(remote, slot.Connection, slotId);
                            }
                        }
                    }
                }
            }

            packet = null;
            return false;   
        }

        private void NewClient(int slot)
        {
            _newClientCallback?.Invoke(slot);
        }

        public Slot FindSlot(IPEndPoint addr, bool comparePorts, out int slotId)
        {
            for (int i = 0; i < Consts.NET_MAX_CLIENTS; i++)
            {
                if (_networkSlots[i].Connection.ConnectionState != ConnectionState.OFFLINE &&
                    _networkSlots[i].Connection.ConnectionState != ConnectionState.ERROR &&
                    _networkSlots[i].Connection.PeerAddr.Address.Equals(addr.Address) && 
                    (!comparePorts || _networkSlots[i].Connection.PeerAddr.Port == addr.Port))
                {
                    slotId = i;
                    return _networkSlots[i];
                }
            }

            slotId = -1;
            return null;
        }

        public void Send(NetChunk chunk)
        {
        }
    }
}

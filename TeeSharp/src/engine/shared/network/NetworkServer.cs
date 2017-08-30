using System;
using System.Net;
using System.Net.Sockets;
using TeeSharp.Server;

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
            _recvUnpacker = new NetworkReceiveUnpacker();
        }

        public AddressFamily NetType()
        {
            return _udpClient.Client.AddressFamily;
        }

        public virtual void Init()
        {
            _netBan = Kernel.Get<ServerBan>();
            _config = Kernel.Get<Configuration>();
        }

        public virtual void Open(IPEndPoint endPoint, int maxClients, int maxClientsPerIp)
        {
            if (!Base.CreateUdpClient(endPoint, out _udpClient))
                throw new Exception($"couldn't open socket. port {_config.GetInt("SvPort")} might already be in use");

            _networkSlots = new Slot[Consts.NET_MAX_CLIENTS];
            SetMaxClientsPerIp(maxClientsPerIp);

            for (var i = 0; i < _networkSlots.Length; i++)
            {
                _networkSlots[i] = new Slot();
                _networkSlots[i].Connection.Init(_udpClient, true);
            }
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
            for (var i = 0; i < Consts.NET_MAX_CLIENTS; i++)
            {
                _networkSlots[i].Connection.Update();
                if (_networkSlots[i].Connection.ConnectionState == ConnectionState.ERROR)
                {
                    if (time - _networkSlots[i].Connection.ConnectionTime < Base.TimeFreq())
                        _netBan.BanAddr(ClientAddr(i), 60, "Stressing network");
                    else
                        Drop(i, _networkSlots[i].Connection.ErrorString());
                }
            }
        }

        public IPEndPoint ClientAddr(int slot)
        {
            return _networkSlots[slot].Connection.PeerAddr;
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
                            for (var i = 0; i < Consts.NET_MAX_CLIENTS; i++)
                            {
                                if (_networkSlots[i].Connection.ConnectionState == ConnectionState.OFFLINE)
                                    continue;

                                if (Base.CompareAddresses(_networkSlots[i].Connection.PeerAddr,
                                    remote, false))
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
                            for (var i = 0; i < Consts.NET_MAX_CLIENTS; i++)
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
            for (var i = 0; i < Consts.NET_MAX_CLIENTS; i++)
            {
                if (_networkSlots[i].Connection.ConnectionState != ConnectionState.OFFLINE &&
                    _networkSlots[i].Connection.ConnectionState != ConnectionState.ERROR &&
                    Base.CompareAddresses(_networkSlots[i].Connection.PeerAddr, addr, true))
                {
                    slotId = i;
                    return _networkSlots[i];
                }
            }

            slotId = -1;
            return null;
        }

        public bool Send(NetChunk chunk)
        {
            if (chunk.DataSize >= Consts.NET_MAX_PAYLOAD)
            {
                Base.DbgMessage("network", $"packet payload too big, {chunk.DataSize} bytes. dropping packet");
                return false;
            }

            if ((chunk.Flags & SendFlag.CONNLESS) != 0)
            {
                NetworkBase.SendPacketConnless(_udpClient, chunk.Address, chunk.Data, chunk.DataSize);
                return true;
            }

            Base.DbgAssert(chunk.ClientId >= 0, "errornous client id");
            Base.DbgAssert(chunk.ClientId < Consts.NET_MAX_CLIENTS, "errornous client id");

            var flags = (ChunkFlags) 0;
            if ((chunk.Flags & SendFlag.VITAL) != 0)
                flags = ChunkFlags.VITAL;

            if (_networkSlots[chunk.ClientId].Connection.QueueChunk(flags, chunk.DataSize, chunk.Data))
            {
                if ((chunk.Flags & SendFlag.FLUSH) != 0)
                    _networkSlots[chunk.ClientId].Connection.Flush();
            }
            else
            {
                Drop(chunk.ClientId, "Error sending data");
            }

            return true;
        }
    }
}

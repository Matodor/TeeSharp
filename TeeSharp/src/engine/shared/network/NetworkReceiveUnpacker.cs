using System;
using System.Net;

namespace TeeSharp
{
    public class NetworkReceiveUnpacker
    {
        public readonly NetPacketConstruct PacketConstruct;

        private Configuration _config;
        private IPEndPoint _address;
        private NetworkConnection _connection;
        private int _clientId;
        private int _currentChunk;
        private bool _valid;

        public NetworkReceiveUnpacker()
        {
            PacketConstruct = new NetPacketConstruct();
        }

        public void Init()
        {
            _config = Kernel.Get<Configuration>();
        }

        public void Clear()
        {
            _valid = false;
        }

        public void Start(IPEndPoint addr, NetworkConnection connection, int clientId)
        {
            _address = addr;
            _connection = connection;
            _clientId = clientId;
            _currentChunk = 0;
            _valid = true;
        }

        public bool FetchChunk(out NetChunk packet)
        {
            var header = new NetChunkHeader();
            var end = PacketConstruct.DataSize;

            while (true)
            {
                if (!_valid || _currentChunk >= PacketConstruct.NumChunks)
                {
                    Clear();
                    packet = null;
                    return false;
                }

                var dataIndex = 0;
                for (var i = 0; i < _currentChunk; i++)
                {
                    dataIndex = header.Unpack(PacketConstruct.ChunkData, dataIndex);
                    dataIndex += header.Size;
                }

                // unpack the header
                dataIndex = header.Unpack(PacketConstruct.ChunkData, dataIndex);
                _currentChunk++;

                if (dataIndex + header.Size > end)
                {
                    Clear();
                    packet = null;
                    return false;
                }

                if (_connection != null && (header.Flags & ChunkFlags.VITAL) != 0)
                {
                    if (header.Sequence == (_connection.Ack + 1) % Consts.NET_MAX_SEQUENCE)
                    {
                        _connection.Ack = (_connection.Ack + 1) % Consts.NET_MAX_SEQUENCE;
                    }
                    else
                    {
                        if (NetworkBase.IsSeqInBackroom(header.Sequence, _connection.Ack))
                            continue;

                        if (_config.GetInt("Debug") != 0)
                            Base.DbgMessage("conn", $"asking for resend {header.Sequence} {(_connection.Ack + 1) % Consts.NET_MAX_SEQUENCE}");
                        _connection.SignalResend();
                        continue;
                    }
                }

                packet = new NetChunk
                {
                    ClientId = _clientId,
                    Address = _address,
                    Flags = 0,
                    DataSize = header.Size,
                    Data = new byte[header.Size]
                };
                Array.Copy(PacketConstruct.ChunkData, dataIndex, packet.Data, 0, header.Size);
                return true;
            }
        }
    }
}
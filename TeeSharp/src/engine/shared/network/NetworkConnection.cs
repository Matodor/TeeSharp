using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace TeeSharp
{
    public class NetworkConnection
    {
        public ConnectionState ConnectionState { get; private set; }
        public long ConnectionTime { get; private set; }
        public IPEndPoint PeerAddr { get; private set; }

        private readonly NetPacketConstruct _packetConstruct;
        private readonly Queue<NetChunkResend> _resendBuffer;

        private Configuration _config;
        private UdpClient _udpClient;
        private bool _blockCloseMsg;
        private bool _remoteClosed;

        private string _errorString;

        private int _bufferSize;
        private int _ack;
        private int _secuence;

        private long _lastSendTime;
        private long _lastReceiveTime;
        private long _lastUpdateTime;

        public NetworkConnection()
        {
            _packetConstruct = new NetPacketConstruct
            {
                ChunkData = new byte[Consts.NET_MAX_PAYLOAD],
                DataSize = 0,
                Flags = 0,
                NumChunks = 0,
                Ack = 0
            };
            _resendBuffer = new Queue<NetChunkResend>();    
        }

        private void ResetPacketConstruct()
        {
            _packetConstruct.DataSize = 0;
            _packetConstruct.Ack = 0;
            _packetConstruct.Flags = 0;
            _packetConstruct.NumChunks = 0;
        }

        public void ResetStats()
        {
            
        }

        public void Reset()
        {
            ConnectionTime = 0;
            ConnectionState = ConnectionState.OFFLINE;

            _secuence = 0;
            _ack = 0;
            _remoteClosed = false;

            _lastSendTime = 0;
            _lastReceiveTime = 0;
            _lastUpdateTime = 0;

            PeerAddr = new IPEndPoint(IPAddress.Any, 0);

            _errorString = "";
            _bufferSize = 0;
            _resendBuffer.Clear();

            ResetPacketConstruct();
        }

        public void SetError(string error)
        {
            _errorString = error;
        }

        public void Init(UdpClient client, bool blockCloseMsg)
        {
            Reset();
            ResetStats();

            _config = Kernel.Get<Configuration>();
            _udpClient = client;
            _blockCloseMsg = blockCloseMsg;
            _errorString = "";
        }

        public void AckChunks(int ack)
        {
            while (true)
            {
                if (_resendBuffer.Count == 0)
                    break;

                var resendChunk = _resendBuffer.Peek();
                if (resendChunk == null)
                    break;

                if (NetworkBase.IsSeqInBackroom(resendChunk.Sequence, ack))
                {
                    _resendBuffer.Dequeue();
                    _bufferSize -= Marshal.SizeOf<NetworkChange>() + resendChunk.DataSize;
                }
                else
                    return;
            }
        }

        public void SignalResend()
        {
            _packetConstruct.Flags |= PacketFlag.RESEND;
        }

        public int Flush()
        {
            var numChunks = _packetConstruct.NumChunks;
            if (numChunks == 0 && _packetConstruct.Flags == 0)
                return 0;

            _packetConstruct.Ack = _ack;
            NetworkBase.SendPacket(_udpClient, PeerAddr, _packetConstruct);
            _lastSendTime = Base.TimeGet();
            ResetPacketConstruct();
            return numChunks;
        }

        public bool QueueChunkEx(ChunkFlags flags, int dataSize, byte[] data, int sequence)
        {
            if (_packetConstruct.DataSize + dataSize + Consts.NET_MAX_CHUNKHEADERSIZE > Consts.NET_MAX_PAYLOAD)
                Flush();

            var header = new NetChunkHeader
            {
                Flags = flags,
                Size = dataSize,
                Sequence = sequence
            };

            var chunkDataIndex = _packetConstruct.DataSize;
            chunkDataIndex = header.Pack(_packetConstruct.ChunkData, chunkDataIndex);

            Array.Copy(data, 0, _packetConstruct.ChunkData, chunkDataIndex, dataSize);
            chunkDataIndex += dataSize;

            _packetConstruct.NumChunks++;
            _packetConstruct.DataSize = chunkDataIndex;

            if ((flags & ChunkFlags.VITAL) != 0 && (flags & ChunkFlags.RESEND) == 0)
            {
                _bufferSize += Marshal.SizeOf<NetChunkResend>() + dataSize;
                if (_bufferSize >= Consts.NET_CONN_BUFFERSIZE)
                {
                    Disconnect("too weak connection (out of buffer)");
                    return false;
                }

                var resend = new NetChunkResend
                {
                    Sequence = sequence,
                    Flags = flags,
                    DataSize = dataSize,
                    Data = new byte[dataSize],
                    FirstSendTime = Base.TimeGet(),
                    LastSendTime = Base.TimeGet()
                };

                Array.Copy(data, 0, resend.Data, 0, dataSize);
                _resendBuffer.Enqueue(resend);
            }

            return true;
        }

        public bool QueueChunk(ChunkFlags flags, int dataSize, byte[] data)
        {
            if ((flags & ChunkFlags.VITAL) != 0)
                _secuence = (_secuence + 1) % Consts.NET_MAX_SEQUENCE;
            return QueueChunkEx(flags, dataSize, data, _secuence);
        }

        public void SendControl(ControlMessage message, string extra)
        {
            _lastSendTime = Base.TimeGet();
            NetworkBase.SendControlMsg(_udpClient, PeerAddr, _ack, message, extra);
        }

        public void ResendChunk(NetChunkResend resend)
        {
            QueueChunkEx(resend.Flags | ChunkFlags.RESEND, resend.DataSize, resend.Data,
                resend.Sequence);
            resend.LastSendTime = Base.TimeGet();
        }

        public void Resend()
        {
            foreach (var chunk in _resendBuffer)
                ResendChunk(chunk);
        }

        public bool Connect(IPEndPoint addr)
        {
            if (ConnectionState != ConnectionState.OFFLINE)
                return false;

            Reset();
            PeerAddr = addr;
            ConnectionState = ConnectionState.CONNECT;

            SendControl(ControlMessage.CONNECT, "");
            return true;
        }

        public void Disconnect(string reason)
        {
            if (ConnectionState == ConnectionState.OFFLINE)
                return;

            if (!_remoteClosed)
            {
                SendControl(ControlMessage.CLOSE, reason);

                if (!string.IsNullOrEmpty(reason))
                    _errorString = reason;
                
                if (_config.GetInt("Debug") != 0)
                    Base.DbgMessage("network", $"disconnect, reason={reason}");
            }

            Reset();
        }

        public bool Feed(NetPacketConstruct packet, IPEndPoint addr)
        {
            var time = Base.TimeGet();

            if ((packet.Flags & PacketFlag.RESEND) != 0)
                Resend();

            if ((packet.Flags & PacketFlag.CONTROL) != 0)
            {
                var controlMessage = (ControlMessage) packet.ChunkData[0];
                if (controlMessage == ControlMessage.CLOSE)
                {
                    if (!Base.CompareAddresses(PeerAddr, addr, true))
                        return false;
                    ConnectionState = ConnectionState.ERROR;
                    _remoteClosed = true;

                    var str = "";
                    if (packet.DataSize > 1)
                    {
                        str = Encoding.UTF8.GetString(packet.ChunkData, 1, 
                            packet.DataSize < 128 ? packet.DataSize - 1 : 128);
                        str = Base.StrSanitizeStrong(str);
                    }

                    if (_blockCloseMsg)
                        SetError(str);

                    if (_config.GetInt("Debug") != 0)
                        Base.DbgMessage("conn", $"closed reason='{str}'");

                    return false;
                }
                
                if (ConnectionState == ConnectionState.OFFLINE)
                {
                    if (controlMessage == ControlMessage.CONNECT)
                    {
                        Reset();
                        ConnectionState = ConnectionState.PENDING;
                        PeerAddr = addr;
                        _lastSendTime = time;
                        _lastReceiveTime = time;
                        _lastUpdateTime = time;

                        SendControl(ControlMessage.CONNECTACCEPT, "");
                        if (_config.GetInt("Debug") != 0)
                            Base.DbgMessage("connection", "got connection, sending connect+accept", ConsoleColor.Green);
                    }
                }
                else if (ConnectionState == ConnectionState.CONNECT)
                {
                    if (controlMessage == ControlMessage.CONNECTACCEPT)
                    {
                        _lastReceiveTime = time;
                        SendControl(ControlMessage.ACCEPT, "");
                        ConnectionState = ConnectionState.ONLINE;

                        if (_config.GetInt("Debug") != 0)
                            Base.DbgMessage("connection", "got connect+accept, sending accept. connection online", 
                                ConsoleColor.Green);
                    }
                }
            }
            else
            {
                if (ConnectionState == ConnectionState.PENDING)
                {
                    _lastReceiveTime = time;
                    ConnectionState = ConnectionState.ONLINE;

                    if (_config.GetInt("Debug") != 0)
                        Base.DbgMessage("connection", "connecting online", ConsoleColor.Green);
                }
            }

            if (ConnectionState == ConnectionState.ONLINE)
            {
                _lastReceiveTime = time;
                AckChunks(packet.Ack);
            }

            return true;
        }

        public void Update()
        {
            var time = Base.TimeGet();
            
            if (ConnectionState == ConnectionState.OFFLINE || 
                ConnectionState == ConnectionState.ERROR)
                return;

            if (ConnectionState != ConnectionState.OFFLINE &&
                ConnectionState != ConnectionState.CONNECT &&
                (time - _lastReceiveTime) > Base.TimeFreq() * _config.GetInt("ConnTimeout"))
            {
                ConnectionState = ConnectionState.ERROR;
                SetError("Timeout");
            }

            if (_resendBuffer.Count > 0)
            {
                var resend = _resendBuffer.Peek();
                if (time - resend.FirstSendTime > Base.TimeFreq() * _config.GetInt("ConnTimeout"))
                {
                    ConnectionState = ConnectionState.ERROR;
                    SetError($"Too weak connection (not acked for {_config.GetInt("ConnTimeout")} seconds)");
                }
                else
                {
                    if (time - resend.LastSendTime > Base.TimeFreq())
                        ResendChunk(resend);
                }
            }

            if (ConnectionState == ConnectionState.ONLINE)
            {
                if (time - _lastSendTime > Base.TimeFreq() / 2)
                {
                    var flushedChunks = Flush();
                    if (flushedChunks != 0 && _config.GetInt("Debug") != 0)
                        Base.DbgMessage("connection", $"flushed connection due to timeout. {flushedChunks} chunks.");
                }

                if (time - _lastSendTime > Base.TimeFreq())
                    SendControl(ControlMessage.KEEPALIVE, "");
            }
            else if (ConnectionState == ConnectionState.CONNECT)
            {
                if (time - _lastSendTime > Base.TimeFreq() / 2)
                    SendControl(ControlMessage.CONNECT, "");
            }
            else if (ConnectionState == ConnectionState.PENDING)
            {
                if (time - _lastSendTime > Base.TimeFreq() / 2)
                    SendControl(ControlMessage.CONNECTACCEPT, "");
            }
        }

        public string ErrorString()
        {
            return _errorString;
        }
    }
}
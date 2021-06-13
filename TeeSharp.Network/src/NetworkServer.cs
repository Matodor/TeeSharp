using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Serilog;
using TeeSharp.Core.Helpers;
using TeeSharp.Core.MinIoC;

namespace TeeSharp.Network
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class NetworkServer : BaseNetworkServer
    {
        protected Dictionary<int, int> MapClients { get; set; }
        
        public override void Init(NetworkServerConfig config)
        {
            Config = config;
            ChunkFactory = Container.Resolve<BaseChunkFactory>();
            ChunkFactory.Init();
            MapClients = new Dictionary<int, int>(Config.MaxConnections);
            Connections = Enumerable.Range(0, Config.MaxConnections)
                .Select(x => Container.Resolve<BaseNetworkConnection>())
                .ToArray();
        }

        public override void Update()
        {
            
        }

        // ReSharper disable once InconsistentNaming
        public override bool Open(IPEndPoint localEP)
        {
            if (localEP == null)
                throw new ArgumentNullException(nameof(localEP));
            
            if (!NetworkHelper.TryGetUdpClient(localEP, out var socket))
                return false;

            Socket = socket;
            Socket.Client.Blocking = true;
            RefreshSecurityTokenSeed();
            
            return true;
        }

        public override bool Receive(out NetworkMessage netMsg, ref SecurityToken responseToken)
        {
            if (ChunkFactory.TryGetMessage(out netMsg))
                return true;

            // ReSharper disable once InconsistentNaming
            var endPoint = default(IPEndPoint);
            var data = Socket.Receive(ref endPoint).AsSpan();

            if (data.Length == 0)
                return false;

            // TODO
            // Check for banned IP address

            var isSixUp = false;
            var securityToken = default(SecurityToken);

            responseToken = SecurityToken.Unknown;

            if (!NetworkHelper.TryUnpackPacket(
                data,
                ChunkFactory.NetworkPacket,
                ref isSixUp,
                ref securityToken,
                ref responseToken))
            {
                return false;
            }

            if (ChunkFactory.NetworkPacket.Flags.HasFlag(PacketFlags.ConnectionLess))
            {
                if (isSixUp && securityToken != GetToken(endPoint))
                    return false;

                netMsg = new NetworkMessage
                {
                    ClientId = -1,
                    EndPoint = endPoint,
                    Flags = MessageFlags.ConnectionLess,
                    Data = new byte[ChunkFactory.NetworkPacket.DataSize],
                    ExtraData = null,
                };

                ChunkFactory.NetworkPacket.Data
                    .AsSpan()
                    .Slice(0, ChunkFactory.NetworkPacket.DataSize)
                    .CopyTo(netMsg.Data);

                if (ChunkFactory.NetworkPacket.Flags.HasFlag(PacketFlags.Extended))
                {
                    netMsg.Flags |= MessageFlags.Extended;
                    netMsg.ExtraData = new byte[NetworkConstants.PacketExtraDataSize];
                    
                    ChunkFactory.NetworkPacket.ExtraData
                        .AsSpan()
                        .CopyTo(netMsg.ExtraData);
                }

                return true;
            }

            if (ChunkFactory.NetworkPacket.DataSize == 0 &&
                ChunkFactory.NetworkPacket.Flags.HasFlag(PacketFlags.ConnectionState))
            {
                return false;
            }

            var connectionId = GetConnectionId(endPoint);
            if (connectionId == -1)
            {
                if (isSixUp)
                {
                    throw new NotImplementedException();
                }
                
                if (IsConnStateMsgWithToken(ChunkFactory.NetworkPacket))
                {
                    ProcessConnStateMsgWithToken(endPoint, ChunkFactory.NetworkPacket);
                    return false;
                }
                
                throw new NotImplementedException();
            }
            else
            {
                if (!isSixUp && Connections[connectionId].IsSixUp)
                {
                    throw new NotImplementedException();
                }
                
                throw new NotImplementedException();
            }
            
            return true;
        }

        /// <summary>
        /// Get token for given End Point.
        /// TODO: security tests needed
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public override SecurityToken GetToken(IPEndPoint endPoint)
        {
            const int offset = sizeof(int);
            var buffer = (Span<byte>) new byte[offset + SecurityTokenSeed.Length];
            Unsafe.As<byte, int>(ref buffer[0]) = endPoint.GetHashCode();
            SecurityTokenSeed.CopyTo(buffer.Slice(offset));
            
            return SecurityHelper.KnuthHash(buffer).GetHashCode();
        }

        public override int GetConnectionId(IPEndPoint endPoint)
        {
            if (MapClients.TryGetValue(endPoint.GetHashCode(), out var id))
                return id;
            return -1;
        }

        public override bool HasConnection(IPEndPoint endPoint)
        {
            return MapClients.ContainsKey(endPoint.GetHashCode());
        }

        public override void SendConnStateMsg(IPEndPoint endPoint, ConnectionStateMsg connState, 
            SecurityToken token, int ack = 0, bool isSixUp = false, string msg = null)
        {
            NetworkHelper.SendConnStateMsg(Socket, endPoint, connState, token, ack, isSixUp, msg);
        }

        public override void SendConnStateMsg(IPEndPoint endPoint, ConnectionStateMsg connState, 
            SecurityToken token, int ack = 0, bool isSixUp = false, Span<byte> extraData = default)
        {
            NetworkHelper.SendConnStateMsg(Socket, endPoint, connState, token, ack, isSixUp, extraData);
        }

        protected virtual bool IsConnStateMsgWithToken(NetworkPacket packet)
        {
            if (ChunkFactory.NetworkPacket.DataSize == 0 ||
                !ChunkFactory.NetworkPacket.Flags.HasFlag(PacketFlags.ConnectionState))
            {
                return false;
            }

            if (packet.Data[0] == (int) ConnectionStateMsg.Connect &&
                packet.DataSize >= 1 + TypeHelper<SecurityToken>.Size * 2 &&
                packet.Data.AsSpan(1, TypeHelper<SecurityToken>.Size) == SecurityToken.Magic)
            {
                return true;
            }
                
            if (packet.Data[0] == (int) ConnectionStateMsg.Accept &&
                packet.DataSize >= 1 + TypeHelper<SecurityToken>.Size)
            {
                return true;
            }
            
            return false;
        }

        /**
         * Note: Dont use this method on existing connections for the specified `endPoint`
         */
        protected virtual void ProcessConnStateMsgWithToken(IPEndPoint endPoint, NetworkPacket packet)
        {
            var msg = (ConnectionStateMsg) packet.Data[0];
            switch (msg)
            {
                case ConnectionStateMsg.Connect:
                    var token = GetToken(endPoint);
                    SendConnStateMsg(endPoint, ConnectionStateMsg.ConnectAccept, 
                        token, extraData: SecurityToken.Magic);
                    break;
                
                case ConnectionStateMsg.Accept:
                    break;
                
                default:
                    Log.Debug("[network] {Func}: Try process wrong msg type ({Code})", 
                        nameof(ProcessConnStateMsgWithToken), packet.Data[0]);
                    break;
            }
        }
        
        protected override void OnConfigChanged(NetworkServerConfig oldConfig)
        {
            
        }

        protected override void RefreshSecurityTokenSeed()
        {
            SecurityTokenSeed = new byte[12];
            RandomNumberGenerator.Create().GetBytes(SecurityTokenSeed);
        }
    }
}
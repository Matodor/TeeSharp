using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using TeeSharp.Core;
using TeeSharp.Core.Helpers;
using TeeSharp.Network.Abstract;

namespace TeeSharp.Network.Concrete;

public class NetworkConnection : INetworkConnection
{
    public ConnectionState State { get; protected set; }
    public IPEndPoint EndPoint { get; protected set; }

    protected UdpClient Socket { get; set; }
    protected ILogger Logger { get; set; }

    protected SecurityToken SecurityToken { get; set; }
    protected int Sequence;
    protected int PeerAck;
    protected int Ack;
    protected DateTime LastReceiveTime;

    public NetworkConnection(
        UdpClient socket,
        ILogger? logger = null)
    {
        Logger = logger ?? Tee.LoggerFactory.CreateLogger("NetworkConnection");
        Socket = socket;
        EndPoint = null!;
    }

    public virtual void Init(IPEndPoint endPoint, SecurityToken securityToken)
    {
        Reset();

        State = ConnectionState.Online;
        EndPoint = endPoint;
        SecurityToken = securityToken;
    }

    /// <summary>
    /// Process packet and return chunks
    /// </summary>
    /// <param name="endPoint"></param>
    /// <param name="packet"></param>
    /// <returns></returns>
    public IEnumerable<NetworkMessage> ProcessPacket(IPEndPoint endPoint, NetworkPacketIn packet)
    {
        if (Sequence > PeerAck)
        {
            if (packet.Ack < PeerAck ||
                packet.Ack > Sequence)
            {
                yield break;
            }
        }
        else
        {
            if (packet.Ack < PeerAck &&
                packet.Ack > Sequence)
            {
                yield break;
            }
        }

        PeerAck = packet.Ack;
        var data = packet.Data.AsSpan();

        if (State != ConnectionState.Offline &&
            SecurityToken != SecurityToken.Unknown &&
            SecurityToken != SecurityToken.Unsupported)
        {
            if (data.Length < StructHelper<SecurityToken>.Size)
                yield break;

            var tokenOffset = data.Length - StructHelper<SecurityToken>.Size;
            var token = (SecurityToken)data.Slice(tokenOffset);
            data = data.Slice(0, tokenOffset);

            if (SecurityToken != token)
            {
                Logger.LogDebug(
                    "Token mismatch: '{TokenExpected}' != '{TokenGot}' ({EndPoint})",
                    SecurityToken,
                    token,
                    EndPoint
                );

                yield break;
            }
        }

        if (packet.Flags.HasFlag(NetworkPacketInFlags.Resend))
        {
            ResendChunks();
        }

        if (packet.Flags.HasFlag(NetworkPacketInFlags.Connection))
        {
            var msg = (ConnectionStateMsg)data[0];




        }
        else if (State == ConnectionState.Pending)
        {
            LastReceiveTime = DateTime.UtcNow;
            State = ConnectionState.Online;
            Logger.LogDebug("Connecting online ({EndPoint})", EndPoint);
        }

        if (State == ConnectionState.Online)
        {
            LastReceiveTime = DateTime.UtcNow;
            AckChunks(packet.Ack);
        }

        yield break;
    }

    public bool ProcessConnectionStatePacket(
        IPEndPoint endPoint,
        NetworkPacketIn packet,
        ConnectionStateMsg msg)
    {
        switch (msg)
        {
            case ConnectionStateMsg.Connect when State == ConnectionState.Offline:
                break;

            case ConnectionStateMsg.ConnectAccept when State == ConnectionState.Connect
                break;

            case ConnectionStateMsg.Close:
                if (!EndPoint.Equals(endPoint))
                    return false;

                break;
        }

        return true;
    }

    protected virtual void AckChunks(int ack)
    {
        throw new NotImplementedException();
    }

    protected virtual void ResendChunks()
    {
        throw new NotImplementedException();
    }

    protected virtual void Reset()
    {
        Sequence = 0;
        PeerAck = 0;
        Ack = 0;
    }
}

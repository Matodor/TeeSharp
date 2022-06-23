using System;
using System.Collections.Generic;
using System.Linq;
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
    protected DateTime LastSendTime;
    protected DateTime LastUpdateTime;

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

        LastReceiveTime = DateTime.UtcNow;
        LastSendTime = DateTime.UtcNow;
        LastUpdateTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Process packet and return messages
    /// </summary>
    /// <param name="endPoint"></param>
    /// <param name="packet"></param>
    /// <returns></returns>
    public IEnumerable<NetworkMessage> ProcessPacket(IPEndPoint endPoint, NetworkPacketIn packet)
    {
        if (State == ConnectionState.Offline)
            return Enumerable.Empty<NetworkMessage>();

        if (Sequence > PeerAck)
        {
            if (packet.Ack < PeerAck ||
                packet.Ack > Sequence)
            {
                return Enumerable.Empty<NetworkMessage>();
            }
        }
        else
        {
            if (packet.Ack < PeerAck &&
                packet.Ack > Sequence)
            {
                return Enumerable.Empty<NetworkMessage>();
            }
        }

        PeerAck = packet.Ack;
        var data = packet.Data.AsSpan();

        if (data.Length < StructHelper<SecurityToken>.Size)
            return Enumerable.Empty<NetworkMessage>();

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

            return Enumerable.Empty<NetworkMessage>();
        }

        if (packet.Flags.HasFlag(NetworkPacketInFlags.Resend))
        {
            ResendMessages();
        }

        if (packet.Flags.HasFlag(NetworkPacketInFlags.Connection))
        {
            var msg = (ConnectionStateMsg)data[0];

            if (ProcessConnectionStateMsg(endPoint, packet, msg) == false)
                return Enumerable.Empty<NetworkMessage>();
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
            AckMessages(packet.Ack);
        }

        return GetMessagesFromPacket(packet.NumberOfMessages, data);
    }

    public IEnumerable<NetworkMessage> GetMessagesFromPacket(
        int numberOfMessages,
        Span<byte> data)
    {
        if (data.IsEmpty || numberOfMessages <= 0)
        {
            return Enumerable.Empty<NetworkMessage>();
        }

        var header = new NetworkMessageHeader();
        var messages = new List<NetworkMessage>(numberOfMessages);
        var message = 0;

        for (int i = 0; i < numberOfMessages; i++)
        {

        }

        return messages;
    }

    public bool ProcessConnectionStateMsg(
        IPEndPoint endPoint,
        NetworkPacketIn packet,
        ConnectionStateMsg msg)
    {
        switch (msg)
        {
            case ConnectionStateMsg.Connect when State == ConnectionState.Offline:
                break;

            case ConnectionStateMsg.ConnectAccept when State == ConnectionState.Connect:
                break;

            case ConnectionStateMsg.Close:
                if (!EndPoint.Equals(endPoint))
                    return false;

                break;
        }

        return true;
    }

    protected virtual void AckMessages(int ack)
    {
        throw new NotImplementedException();
    }

    protected virtual void ResendMessages()
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

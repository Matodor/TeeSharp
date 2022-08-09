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
    public int Id { get; }
    public ConnectionState State { get; protected set; }
    public IPEndPoint EndPoint { get; protected set; }

    protected UdpClient Socket { get; set; }
    protected ILogger Logger { get; set; }

    protected SecurityToken SecurityToken { get; set; }
    protected ConnectionSettings Settings { get; private set; }

    protected int Sequence;
    protected int PeerAck;
    protected int Ack;

    protected DateTime LastReceiveTime;
    protected DateTime LastSendTime;
    protected DateTime LastUpdateTime;
    protected readonly NetworkPacketAccumulator MessageAccumulator;

    public NetworkConnection(
        int id,
        UdpClient socket,
        ConnectionSettings settings,
        ILogger? logger = null)
    {
        Id = id;
        Logger = logger ?? Tee.LoggerFactory.CreateLogger("NetworkConnection");
        Socket = socket;
        EndPoint = null!;
        Settings = settings;
        MessageAccumulator = new NetworkPacketAccumulator();
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

    public void Disconnect(string reason)
    {
        throw new NotImplementedException();
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

        if (packet.Flags.HasFlag(NetworkPacketFlags.Connection))
        {
            var msg = (ConnectionStateMsg)data[0];

            if (ProcessConnectionStateMsg(endPoint, data.Slice(1), msg) == false)
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

    public void Update()
    {
        if (State is ConnectionState.Offline)
            return;

        var now = DateTime.UtcNow;
        var isActive = State is ConnectionState.Pending or ConnectionState.Online;
        if (isActive && now - LastReceiveTime > TimeSpan.FromSeconds(Settings.Timeout))
        {

        }
    }

    public void FlushMessages()
    {
        throw new NotImplementedException();
    }

    public bool QueueMessage(Span<byte> data, NetworkMessageFlags flags)
    {
        if (flags.HasFlag(NetworkMessageFlags.Vital))
            Sequence = (Sequence + 1) % NetworkConstants.MaxSequence;

        return QueueMessageInternal(data, flags, false);
    }

    protected bool QueueMessageInternal(
        Span<byte> data,
        NetworkMessageFlags flags,
        bool saveToResend)
    {
        if (State != ConnectionState.Online && State != ConnectionState.Pending)
            return false;

        var needSize = MessageAccumulator.BufferSize + data.Length + NetworkConstants.PacketHeaderSize;
        var availableSize = MessageAccumulator.Buffer.Length - StructHelper<SecurityToken>.Size;

        if (needSize > availableSize)
            FlushMessages();

        var header = new NetworkMessageHeader(flags, data.Length, Sequence);
        var buffer = MessageAccumulator.Buffer.AsSpan(MessageAccumulator.BufferSize);

        buffer = header.Pack(buffer);
        data.CopyTo(buffer);

        MessageAccumulator.BufferSize += MessageAccumulator.Buffer.Length - (buffer.Length - data.Length);
        MessageAccumulator.NumberOfMessages++;

        if (flags.HasFlag(NetworkMessageFlags.Vital) && saveToResend)
        {
            throw new NotImplementedException();
        }

        return true;
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

        for (var i = 0; i < numberOfMessages; i++)
        {
            data = header.Unpack(data);

            if (data.Length < header.Size)
                return messages;

            if (header.Flags.HasFlag(NetworkMessageFlags.Vital))
            {
                if (header.Sequence == (Ack + 1) % NetworkConstants.MaxSequence)
                {
                    Ack = header.Sequence;
                }
                else
                {
                    if (NetworkHelper.IsSequenceInBackroom(header.Sequence, Ack))
                        continue;

                    Logger.LogDebug("Asking for resend {Sequence} {Ack}",
                        header.Sequence, (Ack + 1) % NetworkConstants.MaxSequence);
                    ResendMessages();
                    continue;
                }
            }

            messages.Add(new NetworkMessage(
                connectionId: Id,
                endPoint: EndPoint,
                flags: header.Flags,
                data: data.Slice(0, header.Size).ToArray(),
                extraData: Array.Empty<byte>()
            ));

            data = data.Slice(header.Size);
        }

        return messages;
    }

    public bool ProcessConnectionStateMsg(
        IPEndPoint endPoint,
        Span<byte> data,
        ConnectionStateMsg msg)
    {
        switch (msg)
        {
            case ConnectionStateMsg.Close:
                if (!EndPoint.Equals(endPoint))
                    return false;

                State = ConnectionState.Disconnecting;
                Logger.LogDebug("Connection closed: {ConnectionId}", Id);
                break;
        }

        return true;
    }

    protected virtual void AckMessages(int ack)
    {
        // TODO implement this
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

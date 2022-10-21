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
    public const int MaxResendBufferSize = 1024 * 32;

    public int Id { get; }
    public ConnectionState State { get; protected set; }
    public IPEndPoint EndPoint { get; protected set; }

    protected UdpClient Socket { get; set; }
    protected ILogger Logger { get; set; }

    protected SecurityToken SecurityToken { get; set; }
    protected ConnectionSettings Settings { get; private set; }

    protected int Sequence { get; set; }
    protected int PeerAck { get; set; }
    protected int Ack { get; set; }

    protected DateTime LastReceiveTime { get; set; }
    protected DateTime LastSendTime { get; set; }
    protected DateTime LastUpdateTime { get; set; }

    protected PacketAccumulator MessageAccumulator { get; set; }
    protected Queue<MessageForResend> MessagesForResend { get; set; }
    protected int MessagesForResendDataSize { get; set; }

    protected class PacketAccumulator
    {
        public NetworkPacketFlags Flags { get; set; } = NetworkPacketFlags.None;
        public int NumberOfMessages { get; set; }
        public int BufferSize { get; set; }
        public readonly byte[] Buffer = new byte[NetworkConstants.MaxPayload];

        public void Reset()
        {
            BufferSize = 0;
            NumberOfMessages = 0;
        }
    }

    protected class MessageForResend
    {
        public static readonly int SizeOf =
            sizeof(NetworkMessageHeaderFlags) +
            sizeof(int) +
            StructHelper<IntPtr>.Size +
            StructHelper<DateTime>.Size +
            StructHelper<DateTime>.Size;

        public NetworkMessageHeaderFlags Flags { get; }
        public int Sequence { get; }
        public byte[] Data { get; }
        public DateTime LastSendTime { get; set; }
        public DateTime FirstSendTime { get; set; }

        public MessageForResend(
            NetworkMessageHeaderFlags flags,
            int sequence,
            byte[] data,
            DateTime lastSendTime,
            DateTime firstSendTime)
        {
            Flags = flags;
            Sequence = sequence;
            Data = data;
            LastSendTime = lastSendTime;
            FirstSendTime = firstSendTime;
        }
    }

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
        MessageAccumulator = new PacketAccumulator();
        MessagesForResend = new Queue<MessageForResend>(32);
        MessagesForResendDataSize = 0;
        State = ConnectionState.Offline;
    }

    public void Init(IPEndPoint endPoint, SecurityToken securityToken)
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
        if (State is ConnectionState.Offline)
            return;

        if (State != ConnectionState.Disconnecting)
        {
            if (string.IsNullOrWhiteSpace(reason))
                SendConnectionStateMsg(ConnectionStateMsg.Close);
            else
                SendConnectionStateMsg(ConnectionStateMsg.Close, reason);
        }

        Reset();
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
                EndPoint.ToString()
            );

            return Enumerable.Empty<NetworkMessage>();
        }

        if (Sequence >= PeerAck)
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

        if (packet.Flags.HasFlag(NetworkPacketFlags.Resend))
            ResendMessages();

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
            Logger.LogDebug("Connecting online ({EndPoint})", EndPoint.ToString());
        }

        if (State == ConnectionState.Online)
        {
            LastReceiveTime = DateTime.UtcNow;
            AckMessages(packet.Ack);
        }

        return GetMessagesFromPacket(packet.NumberOfMessages, data);
    }

    protected void ResendMessages()
    {
        foreach (var messageForResend in MessagesForResend)
            ResendMessage(messageForResend);
    }

    protected void ResendMessage(MessageForResend message)
    {
        if (QueueMessageInternal(message.Data, message.Flags, fromResend: true, message.Sequence))
            message.LastSendTime = DateTime.UtcNow;
    }

    public void Update()
    {
        var isActive = State is ConnectionState.Pending or ConnectionState.Online;
        if (isActive == false)
            return;

        var now = DateTime.UtcNow;
        if (now - LastReceiveTime > TimeSpan.FromSeconds(Settings.Timeout))
        {
            State = ConnectionState.Timeout;
        }

        if (MessagesForResend.TryPeek(out var messagesForResend))
        {
            if (now - messagesForResend.FirstSendTime > TimeSpan.FromSeconds(Settings.Timeout))
            {
                State = ConnectionState.Timeout;
            }
            else if (now - messagesForResend.LastSendTime > TimeSpan.FromSeconds(1))
            {
                ResendMessage(messagesForResend);
            }
        }

        switch (State)
        {
            case ConnectionState.Online:
                if (now - LastSendTime > TimeSpan.FromSeconds(0.5))
                {
                    var flushedMessages = FlushMessages();
                    if (flushedMessages > 0)
                        Logger.LogDebug("Flushed connection due to timeout ({FlushedMessages} messages)", flushedMessages);
                }

                if (now - LastSendTime > TimeSpan.FromSeconds(1))
                    SendConnectionStateMsg(ConnectionStateMsg.KeepAlive);

                break;

            case ConnectionState.Pending:
                if (now - LastSendTime > TimeSpan.FromSeconds(0.5))
                    SendConnectionStateMsg(ConnectionStateMsg.ConnectAccept, SecurityToken.Magic);
                break;
        }
    }

    public int FlushMessages()
    {
        var numberOfMessages = MessageAccumulator.NumberOfMessages;
        if (numberOfMessages == 0)
            return 0;

        var packet = new NetworkPacketOut(
            flags: MessageAccumulator.Flags,
            ack: Ack,
            numberOfMessages: numberOfMessages,
            dataSize: MessageAccumulator.BufferSize
        );

        MessageAccumulator.Buffer
            .AsSpan(0, packet.DataSize)
            .CopyTo(packet.Data);

        NetworkHelper.SendPacket(
            client: Socket,
            endPoint: EndPoint,
            token: SecurityToken,
            packet: packet,
            useCompression: true
        );

        LastSendTime = DateTime.UtcNow;
        MessageAccumulator.Reset();

        return numberOfMessages;
    }

    public bool QueueMessage(Span<byte> data, NetworkMessageHeaderFlags flags)
    {
        if (flags.HasFlag(NetworkMessageHeaderFlags.Vital))
            Sequence = (Sequence + 1) % NetworkConstants.MaxSequence;

        return QueueMessageInternal(data, flags, fromResend: false, Sequence);
    }

    public void SendConnectionStateMsg(ConnectionStateMsg msg, string? extraMsg = null)
    {
        LastSendTime = DateTime.UtcNow;

        NetworkHelper.SendConnectionStateMsg(
            client: Socket,
            endPoint: EndPoint,
            msg: msg,
            token: SecurityToken,
            ack: Ack,
            extraMsg: extraMsg
        );
    }

    public void SendConnectionStateMsg(ConnectionStateMsg msg, byte[] extraData)
    {
        LastSendTime = DateTime.UtcNow;

        NetworkHelper.SendConnectionStateMsg(
            client: Socket,
            endPoint: EndPoint,
            msg: msg,
            token: SecurityToken,
            ack: Ack,
            extraData: extraData
        );
    }

    protected bool QueueMessageInternal(
        Span<byte> data,
        NetworkMessageHeaderFlags flags,
        bool fromResend,
        int sequence)
    {
        if (State != ConnectionState.Online && State != ConnectionState.Pending)
            return false;

        var needSize = MessageAccumulator.BufferSize + data.Length + NetworkConstants.MaxPacketHeaderSize;
        var availableSize = MessageAccumulator.Buffer.Length - StructHelper<SecurityToken>.Size;

        if (needSize > availableSize)
            FlushMessages();

        var header = new NetworkMessageHeader(flags, data.Length, sequence);
        var buffer = MessageAccumulator.Buffer.AsSpan(MessageAccumulator.BufferSize);

        buffer = header.Pack(buffer);
        data.CopyTo(buffer);

        MessageAccumulator.NumberOfMessages++;
        MessageAccumulator.BufferSize +=
            MessageAccumulator.Buffer.Length -
            MessageAccumulator.BufferSize -
            buffer.Length +
            data.Length;

        if (!header.IsVital || fromResend)
            return true;

        var mfrSize = MessageForResend.SizeOf * (MessagesForResend.Count + 1) + MessagesForResendDataSize + data.Length;
        if (mfrSize > MaxResendBufferSize)
            return false;

        var now = DateTime.UtcNow;
        var messageForResend = new MessageForResend(
            flags: flags,
            sequence: sequence,
            data: data.ToArray(),
            lastSendTime: now,
            firstSendTime: now
        );

        MessagesForResend.Enqueue(messageForResend);
        MessagesForResendDataSize += data.Length;

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

            if (header.IsVital)
            {
                if (header.Sequence == (Ack + 1) % NetworkConstants.MaxSequence)
                {
                    Ack = header.Sequence;
                }
                else
                {
                    if (NetworkHelper.IsSequenceInBackroom(header.Sequence, Ack))
                        continue;

                    Logger.LogDebug("Asking for resend {Sequence} {Ack}", header.Sequence, (Ack + 1) % NetworkConstants.MaxSequence);
                    MessageAccumulator.Flags |= NetworkPacketFlags.Resend;
                    continue;
                }
            }

            messages.Add(new NetworkMessage(
                connectionId: Id,
                endPoint: EndPoint,
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

    protected void AckMessages(int ack)
    {
        while (true)
        {
            if (!MessagesForResend.TryPeek(out var messageForResend) ||
                !NetworkHelper.IsSequenceInBackroom(messageForResend.Sequence, ack))
            {
                return;
            }

            MessagesForResend.Dequeue();
        }
    }

    protected void Reset()
    {
        State = ConnectionState.Offline;
        Sequence = 0;
        PeerAck = 0;
        Ack = 0;

        LastSendTime = DateTime.MinValue;
        LastReceiveTime = DateTime.MinValue;

        MessageAccumulator.Reset();
        MessagesForResend.Clear();
        MessagesForResendDataSize = 0;
    }
}

using System;
using System.Diagnostics.CodeAnalysis;
using TeeSharp.Network.Abstract;

namespace TeeSharp.Network.Concrete;

public class NetworkPacketUnpackerSixup : INetworkPacketUnpacker
{
    public bool TryUnpack(Span<byte> buffer, [NotNullWhen(true)] out NetworkPacketIn? packet)
    {
        if (buffer.Length is < NetworkConstants.PacketHeaderSize or > NetworkConstants.MaxPacketSize)
        {
            packet = null;
            return false;
        }

        var flags = (PacketFlags) (buffer[0] >> 2);

        return flags.HasFlag(PacketFlags.ConnectionLess)
            ? TryUnpackConnectionLessPacket(buffer, out packet)
            : TryUnpackConnectionPacket(flags, buffer, out packet);
    }

    protected virtual bool TryUnpackConnectionPacket(
        PacketFlags flags,
        Span<byte> buffer,
        [NotNullWhen(true)] out NetworkPacketIn? packet)
    {
        var isSixup = flags.HasFlag(PacketFlags.Unused);
        var dataStart = isSixup
            ? NetworkConstants.PacketHeaderSizeSixup
            : NetworkConstants.PacketHeaderSize;

        if (dataStart > buffer.Length)
        {
            packet = null;
            return false;
        }

        var ack = ((buffer[0] & 0b_0000_0011) << 8) | buffer[1];
        var chunksCount = buffer[2];
        var data = new byte[buffer.Length - dataStart];
        var extraData = Array.Empty<byte>();

        if (isSixup)
        {
            var flagsSixup = (PacketFlagsSixup) flags;
            flags = PacketFlags.None;

            if (flagsSixup.HasFlag(PacketFlagsSixup.Connection))
                flags |= PacketFlags.Connection;

            if (flagsSixup.HasFlag(PacketFlagsSixup.Resend))
                flags |= PacketFlags.Resend;

            if (flagsSixup.HasFlag(PacketFlagsSixup.Compression))
                flags |= PacketFlags.Compression;
        }

        if (flags.HasFlag(PacketFlags.Compression))
        {
            if (flags.HasFlag(PacketFlags.Connection))
            {
                packet = null;
                return false;
            }

            throw new NotImplementedException();
        }
        else
        {
            buffer
                .Slice(dataStart, data.Length)
                .CopyTo(data);
        }

        packet = isSixup
            ? new NetworkPacketInSixup(
                flags: flags,
                ack: ack,
                chunksCount: chunksCount,
                data: data,
                extraData: extraData,
                securityToken: (SecurityToken) buffer.Slice(3, 4),
                responseToken: SecurityToken.Unknown
            )
            : new NetworkPacketIn(
                flags: flags,
                ack: ack,
                chunksCount: chunksCount,
                data: data,
                extraData: extraData
            );

        return true;
    }

    protected virtual bool TryUnpackConnectionLessPacket(Span<byte> buffer, [NotNullWhen(true)] out NetworkPacketIn? packet)
    {
        byte[] extraData;

        var flags = PacketFlags.ConnectionLess;
        var isSixup = (buffer[0] & 0b_0000_0011) == 0b_0000_0001;
        var dataStart = isSixup
            ? NetworkConstants.PacketConnectionLessDataOffsetSixup
            : NetworkConstants.PacketConnectionLessDataOffset;

        if (dataStart > buffer.Length)
        {
            packet = null;
            return false;
        }

        var data = new byte[buffer.Length - dataStart];

        buffer
           .Slice(dataStart, data.Length)
           .CopyTo(data);

        if (!isSixup
            && buffer.Slice(0, NetworkConstants.PacketHeaderExtended.Length)
               .SequenceEqual(NetworkConstants.PacketHeaderExtended))
        {
            flags |= PacketFlags.Extended;
            extraData = new byte[NetworkConstants.PacketExtraDataSize];

            buffer
               .Slice(NetworkConstants.PacketHeaderExtended.Length, extraData.Length)
               .CopyTo(extraData);
        }
        else
        {
            extraData = Array.Empty<byte>();
        }

        packet = isSixup
            ? new NetworkPacketInSixup(
                flags: flags,
                ack: 0,
                chunksCount: 0,
                data: data,
                extraData: extraData,
                securityToken: (SecurityToken) buffer.Slice(1, 4),
                responseToken: (SecurityToken) buffer.Slice(5, 4)
            )
            : new NetworkPacketIn(
                flags: flags,
                ack: 0,
                chunksCount: 0,
                data: data,
                extraData: extraData
            );

        return true;
    }
}
